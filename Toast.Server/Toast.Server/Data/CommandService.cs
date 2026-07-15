using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Toast.Core.Commands;
using Toast.Core.Commands.CommandData;
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
        return new AgentResponse();
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
          Console.BackgroundColor = ConsoleColor.Red;
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine( $"### Ошибка сохранения отправленных AgentCommandFor после отправки команд: {exSave}" );
          Console.ResetColor();
        }
        return new AgentResponse { Commands = [.. commandsFor.Select( cf => cf.Command )] };
      }
    }

    public async Task<AgentClient[]> GetAllAgentClients( CancellationToken token = default )
      => await InContext( async dbContext => await dbContext.AgentClient.ToArrayAsync( token ) );

    public async Task<IReadOnlyList<AgentSession>> GetSessions( long[] sessionIds, CancellationToken token = default )
      => await InContext( async dbContext => await dbContext.AgentSession.Where( s => sessionIds.Contains( s.Id ) ).ToListAsync() );

    public async Task<List<AgentCommandFor>> EnqueueCommandAsync( List<string> selectedCommandClients, CommandDataBase commandData, CancellationToken? token = default )
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
            Type = CommandTypes.ShowMessage,
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

      // исключаем принятые результаты из ответа, если вдруг они перепутались между ответами
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
      using var dbContext = await dbFactory.CreateDbContextAsync(token);

      return await dbContext.AgentResultDB.Where( c => c.AgentId == clientId ).AsNoTracking().Include( r => r.Results ).SelectMany( r => r.Results ).FirstAsync( r => r.CommandId == commandId, token );
    }

    public class State
    {
      public ShowMessageData ShowMessageData { get; set; } = new();

      public List<string> SelectedCommandClients { get; set; } = [];

      public string? SelectedDBTablesTypeFullName { get; set; }
    }

    public record ClientInfo( string? remoteIpAddress, int remotePort, int localPort, string? userIdentityName, DateTime time );
  }
}
