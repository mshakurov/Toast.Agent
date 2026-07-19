using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Toast.Core.Commands;
using Toast.Core.Commands.CommandData;
using Toast.Core.Utilities;
using Toast.Server.Api;
using Toast.Server.Data.Models;

namespace Toast.Server.Data
{
  public class CommandService
  {
    public readonly State Current = new State();
    private readonly IDbContextFactory<ApplicationDbContext> dbFactory;

    public event EventHandler<string>? ClientChangedEvent;

    public CommandService( IDbContextFactory<ApplicationDbContext> dbFactory )
    {
      this.dbFactory = dbFactory;
    }

    public List<TestDataItem> GetProtectedData( TestDataItem[] addDefaultItems, ClientInfo? clientInfo = null )
    {
      var list = new List<TestDataItem>
        {
            new(1, "Товар 1", "Секретное значение А"),
            new(2, "Товар 2", "Секретное значение Б"),
        };
      if ( addDefaultItems.Length > 0 )
        list.AddRange( addDefaultItems );
      return list;
    }

    public async Task<AgentResponse> GetCommands( AgentRequest request, ClientInfo? clientInfo = null, CancellationToken token = default )
    {
      //List<AgentCommand> commands =
      //  [
      //    new () { Id = Guid.NewGuid(), Type = CommandTypes.ShowMessage,  JsonParameters = JsonSerializer.Serialize( new ShowMessageData { Title = "Hallow device!", Message  = $"From server! You are: {request.AgentId}", Duration = 11, WaitIfShow = false  } ) }
      //  ];

      using var dbContext = await dbFactory.CreateDbContextAsync();

      var (agentClient, created) = await GetOrCreateClient( dbContext, request.AgentId, clientInfo, token );
      if ( created )
      {
        AgentCommandFor? agentCommandFor = null;
        var agentResponse = new AgentResponse();
        try
        {
          agentCommandFor = ( await EnqueueCommandAsync( [agentClient.ClientId], CommandTypes.GetDeviceInfo, new CommandDataBase(), token ) ).FirstOrDefault();
          if ( agentCommandFor != null )
            agentResponse.Commands = [agentCommandFor.Command];
        }
        catch ( Exception ex )
        {
          ConsoleWriteLineError( $"### Error create command for new Client: {ex.GetFullMessage()}" );
        }

        return agentResponse;
      }
      else
      {
        var now = DateTime.UtcNow;
        agentClient.LastGet = clientInfo?.time ?? now;
        var commandsFor = await dbContext.AgentCommandFor.Include( c => c.Client ).Where( ac => ac.Client.ClientId == request.AgentId && ac.Sent == null ).Include( c => c.Command ).ToListAsync( token );
        commandsFor.ForEach( c => c.Sent = now );
        try
        {
          await dbContext.SaveChangesAsync( token );
        }
        catch ( Exception exSave )
        {
          ConsoleWriteLineError( $"### Ошибка сохранения отправленных AgentCommandFor после отправки команд: {exSave.GetFullMessage()}" );
        }
        return new AgentResponse { Commands = [.. commandsFor.Select( cf => cf.Command )] };
      }
    }

    public async Task<AgentClient[]> GetAllAgentClients( CancellationToken token = default )
      => await InContext( async dbContext => await dbContext.AgentClient.ToArrayAsync( token ) );

    public async Task<IReadOnlyList<AgentSession>> GetSessions( long[] sessionIds, CancellationToken token = default )
      => await InContext( async dbContext => await dbContext.AgentSession.Where( s => sessionIds.Contains( s.Id ) ).ToListAsync() );

    public async Task<List<AgentCommandFor>> EnqueueCommandAsync( List<string> selectedCommandClients, string commandType, CommandDataBase commandData, CancellationToken? token = default )
    {
      List<AgentCommandFor> added = new( selectedCommandClients.Count );

      using var dbContext = await dbFactory.CreateDbContextAsync();

      foreach ( var clientID in selectedCommandClients )
        added.Add( dbContext.AgentCommandFor.Add( new AgentCommandFor()
        {
          ClientId = clientID,
          Command = new AgentCommand()
          {
            Id = Guid.NewGuid(),
            Type = commandType,
            JsonParameters = JsonSerializer.Serialize( commandData, commandData.GetType() )
          }
        } ).Entity );

      await dbContext.SaveChangesAsync( token ?? CancellationToken.None );

      return added;
    }

    internal async Task SetResults( AgentResult agentResult, ClientInfo? clientInfo = null, CancellationToken token = default )
    {
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine( $"SetResults: agentResult: {agentResult.AgentId}, {agentResult.Results.FormatValue()}" );
      Console.ResetColor();

      using var dbContext = await dbFactory.CreateDbContextAsync();

      var (agentClient, created) = await GetOrCreateClient( dbContext, agentResult.AgentId, clientInfo, token );

      agentClient.LastSet = clientInfo?.time ?? DateTime.UtcNow;

      // исключаем принятые результаты из ответа, если вдруг результаты вдруг повторились из-за ошибок на клиенте
      var commandIdHashNew = agentResult.Results.Select( r => r.CommandId ).ToHashSet();
      int duplicateCount = 0;
      foreach ( var r in await dbContext.AgentResultDB.Where( c => c.AgentId == agentResult.AgentId ).AsNoTracking().SelectMany( c => c.Results )
        .Where( r => !commandIdHashNew.Contains( r.CommandId ) )
        .ToListAsync() )
      {
        commandIdHashNew.Remove( r.CommandId );
        duplicateCount++;
      }

      var added = dbContext.AgentResultDB.Add( new AgentResultDB
      {
        AgentId = agentResult.AgentId,
        Results = agentResult.Results.Where( r => commandIdHashNew.Contains( r.CommandId ) ).ToList(),
        Received = DateTime.UtcNow,
      } );

      if ( commandIdHashNew.Count > 0 )
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine( $"SetResults: added: Id {added.Entity.Id}: {added.Entity.AgentId}, Count: {added.Entity.Results.Count}" );
        Console.ResetColor();
      }
      else
      if ( duplicateCount > 0 )
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine( $"SetResults: Duplicated commands: {duplicateCount} (agentId: {added.Entity.AgentId})" );
        Console.ResetColor();
      }

      await dbContext.SaveChangesAsync( token );

      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine( $"SetResults: Saved to DB: {added.Entity.AgentId}, Count: {added.Entity.Results.Count}" );
      Console.ResetColor();
    }

    public async Task<TResult> InContext<TResult>( Func<ApplicationDbContext, Task<TResult>> getter )
    {
      using var dbContext = await dbFactory.CreateDbContextAsync();
      return await getter( dbContext );
    }

    public async Task<(AgentClient client, bool created)> GetOrCreateClient( ApplicationDbContext dbContext, string agentId, ClientInfo? clientInfo = null, CancellationToken token = default )
    {
      var agentClient = await dbContext.AgentClient.FindAsync( [agentId], token );
      bool created = agentClient == null;
      if ( agentClient == null )
        agentClient = dbContext.AgentClient.Add( new Models.AgentClient() { ClientId = agentId } ).Entity;

      var lastSession = new AgentSession { ClientId = agentId, LocalPort = clientInfo?.localPort ?? 0, RemoteIPAddress = clientInfo?.remoteIpAddress ?? string.Empty, RemotePort = clientInfo?.remotePort ?? 0, Time = clientInfo?.time ?? DateTime.UtcNow, UserIdentityName = clientInfo?.userIdentityName };

      dbContext.AgentSession.Add( lastSession );

      // удаляем старше 30 дней
      dbContext.AgentSession.RemoveRange( ( await dbContext.AgentSession.Where( s => s.Time.AddDays( 30 ) < DateTime.UtcNow ).ToArrayAsync() ) );

      await dbContext.SaveChangesAsync( token );

      agentClient.LastAgentSessionId = lastSession.Id;

      await dbContext.SaveChangesAsync( token );

      if ( created )
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine( $"SetResults: Client added: {agentId}" );
        Console.ResetColor();
      }

      ClientChangedEvent?.Invoke( this, agentId );

      return (agentClient, created);
    }

    public async Task<CommandResult?> GetCommandResult( string clientId, Guid commandId, CancellationToken token = default )
    {
      using var dbContext = await dbFactory.CreateDbContextAsync( token );

      return await dbContext.AgentResultDB.Where( c => c.AgentId == clientId ).AsNoTracking().Include( r => r.Results ).SelectMany( r => r.Results ).FirstAsync( r => r.CommandId == commandId, token );
    }

    public async Task<IReadOnlyList<(RemoteServerDB entity, RemoteServer server)>> GetPredefinedServers( ApplicationDbContext dbContext, CancellationToken token = default )
    {
      var dbEntityList = await dbContext.PredefinedServers.AsNoTracking().ToListAsync();

      return dbEntityList.Select( e =>
      {
        try
        {
          var srv = e.GetRemoteServer();
          return (e, srv);
        }
        catch ( Exception ex )
        {
          Console.WriteLine( $"# Invalid {nameof( RemoteServerDB )}.{nameof( RemoteServerDB.Json )}: {ex.GetFullMessage()}" );
          return (e, null);
        }
      } ).Where( d => d.srv != null && d.srv.IsValid() )
      .Select( d => (d.e, srv: ( RemoteServer ) d.srv!) )
      .OrderBy( d => d.srv.BaseUrl ).ThenBy( d => d.srv.APIBasePath ).ThenBy( d => d.srv.LoginModel?.Email ).ThenBy( d => d.srv.LoginModel?.Password )
      .DistinctBy( d => d.srv.GetKey(), StringComparer.InvariantCultureIgnoreCase )
      .ToList();
    }

    public async Task<IReadOnlyList<RemoteServer>> GetPredefinedServers( CancellationToken token = default )
    {
      using var dbContext = await dbFactory.CreateDbContextAsync( token );

      return ( await GetPredefinedServers( dbContext, token ) ).Select( s => s.server ).ToArray();
    }

    public async Task UpdatePredefinedServers( CancellationToken token = default )
    {
      try
      {
        using var dbContext = await dbFactory.CreateDbContextAsync( token );

        var dbEntityList = await dbContext.PredefinedServers.ToListAsync();
        var dbList = await GetPredefinedServers( dbContext, token );

        // удаляем невалидные
        dbContext.PredefinedServers.RemoveRange( dbEntityList.Where( e => !dbList.Any( s => s.entity.Id == e.Id ) ).ToArray() );

        var uiList = ( Current.ChangeSettingsData.AddServers ?? [] ).Union( ( Current.ChangeSettingsData.RemoveServers ?? [] ), RemoteServer.Comparer ).Where( s => s.IsValid() )
          .OrderBy( ui => ui.BaseUrl ).ThenBy( ui => ui.APIBasePath ).ThenBy( ui => ui.LoginModel?.Email ).ThenBy( ui => ui.LoginModel?.Password )
          .ToList();

        // исключаем совпавшие
        uiList = [.. uiList.Where( ui => !dbList.Any( s => RemoteServer.Comparer.Equals( ui, s.server ) ) )];

        dbContext.PredefinedServers.AddRange( uiList.Select( RemoteServerDB.CreateFrom ) );

        await dbContext.SaveChangesAsync( token );
      }
      catch ( Exception ex )
      {
        Console.WriteLine( $"### Error updating PredefinedServers: {ex.GetFullMessage()}" );
      }
    }

    public async Task<CommandResultInfo[]> GetLastDeviceInfos( string commandType, string filter, int maxRows, CancellationToken token = default )
    {
      using var dbContext = await dbFactory.CreateDbContextAsync( token );

      filter = filter.ToLower();

      var query
        = dbContext.AgentResultDB.AsNoTracking()
          .OrderByDescending( r => r.Received ).Include( r => r.Results ).AsNoTracking().SelectMany( r => r.Results.Select( CommandResult => new { r.AgentId, r.Received, CommandResult } ) )
        .Join( dbContext.AgentCommandFor.AsNoTracking().Include( c => c.Command ).Where( c => c.Command.Type == commandType ), r => r.CommandResult.CommandId, c => c.Command.Id, ( r, c ) => new { r.AgentId, r.Received, r.CommandResult, c.Command } )
        .Join( dbContext.AgentClient.AsNoTracking(), r => r.AgentId, c => c.ClientId, ( r, Client ) => new { Client, r.Received, r.CommandResult, r.Command } )
        .GroupJoin( dbContext.AgentSession, r => r.Client.LastAgentSessionId, s => s.Id, ( r, lastSessions ) => new { r.Client, r.Received, r.CommandResult, r.Command, LastSession = lastSessions.FirstOrDefault() } )
        ;

      if ( maxRows >= 1 )
        query = query.Take( maxRows );

      var data = ( await
        query
        .ToListAsync( token ) 
        )
        .Select( r => new CommandResultInfo( r.Client, r.Received, r.CommandResult, r.Command, r.LastSession ) )
        .Where( r => string.IsNullOrEmpty( filter )
          || r.Client.ClientId.ToLower().Contains( filter )
          || r.ClientInfo.ToLower().Contains( filter )
          || r.CommandResultFullInfo.ToLower().Contains( filter )
          || r.LastSessionInfo.ToLower().Contains( filter )
          || r.ReceivedStr.ToLower().Contains( filter )
        )
        .ToArray();

      List<CommandResultInfo> results = [];
      var sessionDict = ( await GetSessions( data.Where( c => c.LastSession is null ).Select( c => c.Client.LastAgentSessionId ).ToArray() ) ).ToDictionary( s => s.Id );
      foreach ( var row in data )
      {
        var lastSession = row.LastSession ?? ( sessionDict.TryGetValue( row.Client.LastAgentSessionId, out var session ) ? session : null );

        results.Add( new CommandResultInfo( row.Client, row.Received, row.CommandResult, row.Command, lastSession ) );
      }

      return results.ToArray();
    }

    void ConsoleWriteLineError( string error )
    {
      Console.BackgroundColor = ConsoleColor.Red;
      Console.ForegroundColor = ConsoleColor.Yellow;
      Console.WriteLine( error );
      Console.ResetColor();

    }

    public class State
    {
      public string HostInfo { get; set; } = string.Empty;

      public object? Commander_SelectedTab { get; set; }

      public object? SendCommand_SelectedTab { get; set; }

      public ShowMessageData ShowMessageData { get; set; } = new();

      public ChangeSettingsData ChangeSettingsData { get; set; } = new() { AddServers = [], RemoveServers = [], PollingInterval = 5 };

      public List<string> SelectedCommandClients { get; set; } = [];

      public string? SelectedDBTablesTypeFullName { get; set; }

      public int ViewCommandResults_MaxRows { get; set; }
      public string ViewCommandResults_Filter { get; set; } = string.Empty;
      public object ViewCommandResults_SelectedCommandType { get; set; } = CommandTypes.GetDeviceInfo;
      public CommandResultInfo[] ViewCommandResults_ResultCollection { get; set; } = [];
    }

    public record ClientInfo( string? remoteIpAddress, int remotePort, int localPort, string? userIdentityName, DateTime time );
  }

  public class CommandResultInfo
  {
    public readonly AgentClient Client;
    public readonly DateTime? Received;
    public readonly CommandResult CommandResult;
    public readonly AgentCommand Command;
    public readonly AgentSession? LastSession;

    public readonly string ClientInfo;
    public readonly string ReceivedStr;
    public readonly string LastSessionInfo;
    public readonly string CommandResultShortInfo;
    public readonly string CommandResultFullInfo;

    public CommandResultInfo( AgentClient Client, DateTime? Received, CommandResult CommandResult, AgentCommand Command, AgentSession? LastSession )
    {
      this.Client = Client;
      this.Received = Received;
      this.CommandResult = CommandResult;
      this.Command = Command;
      this.LastSession = LastSession;

      ClientInfo = $@"{Client.ClientId}
Get/Set: {Client.LastGet:s}/{Client.LastSet:s}";

      ReceivedStr = Received is not null ? Received.Value.ToString("s") : string.Empty;

      LastSessionInfo = string.Empty;
      if (LastSession != null)
        LastSessionInfo = @$"{LastSession.RemoteIPAddress}:{LastSession.LocalPort}@{LastSession.UserIdentityName}";

      CommandResultShortInfo = $"[{( CommandResult.Success ? "!" : "#" )}, {CommandResult.Message.TrimRight( 100 )}]";

      CommandResultFullInfo = $"- [{( CommandResult.Success ? "!" : "#" )}, Сообщ: {CommandResult.Message}]";
    }
  }
}
