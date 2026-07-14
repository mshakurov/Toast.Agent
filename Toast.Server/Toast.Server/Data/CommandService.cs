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

    public event EventHandler<string>? ClientAddedEvent;

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

      var agentClient = await dbContext.AgentClient.FindAsync( [request.AgentId], token );
      if ( agentClient == null )
      {
        agentClient = dbContext.AgentClient.Add( new Models.AgentClient() { ClientId = request.AgentId } ).Entity;
        await dbContext.SaveChangesAsync( token );
        ClientAddedEvent?.Invoke( this, request.AgentId );
        return new AgentResponse();
      }
      else
      {
        var commandsFor = await dbContext.AgentCommandFor.Include( c => c.Client ).Include( c => c.Command ).Where( ac => ac.Client.ClientId == request.AgentId && ac.Sent == null ).ToListAsync( token );
        if ( commandsFor.Count > 0 )
        {
          var now = DateTime.UtcNow;
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
        }
        return new AgentResponse { Commands = [.. commandsFor.Select( cf => cf.Command )] };
      }
    }

    public async Task<AgentClient[]> GetAllAgentClients( CancellationToken token = default )
      => await InContext( async dbContext => await dbContext.AgentClient.ToArrayAsync( token ) );

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
            JsonParameters = JsonSerializer.Serialize( commandData )
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

      var agentClient = await dbContext.AgentClient.FindAsync( [agentResult.AgentId], token );
      if ( agentClient == null )
      {
        agentClient = dbContext.AgentClient.Add( new Models.AgentClient() { ClientId = agentResult.AgentId } ).Entity;
        await dbContext.SaveChangesAsync( token );
        ClientAddedEvent?.Invoke( this, agentResult.AgentId );

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine( $"SetResults: Client added: {agentResult.AgentId}" );
        Console.ResetColor();
      }

      var commandIdHash = agentResult.Results.Select( r => r.CommandId ).ToHashSet();
      foreach ( var r in await dbContext.AgentResultDB.Where( c => c.AgentId == agentResult.AgentId ).Include( c => c.Results ).SelectMany( c => c.Results )
        .Where( r => !commandIdHash.Contains( r.CommandId ) )
        .ToListAsync() )
      {
        commandIdHash.Remove( r.CommandId );
      }

      if ( commandIdHash.Count > 0 )
      {
        var added = dbContext.AgentResultDB.Add( new AgentResultDB { AgentId = agentResult.AgentId, Results = agentResult.Results.Where( r => commandIdHash.Contains( r.CommandId ) ).ToList() } );

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine( $"SetResults: added: Id {added.Entity.Id}: {added.Entity.AgentId}, Count: {added.Entity.Results.Count}" );
        Console.ResetColor();

        await dbContext.SaveChangesAsync( token );

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine( $"SetResults: Saved to DB: {added.Entity.AgentId}, Count: {added.Entity.Results.Count}" );
        Console.ResetColor();
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine( $"SetResults: Duplicate result: {agentResult.AgentId}, Count: {agentResult.Results.Count}" );
        Console.ResetColor();
      }
    }

    public async Task<TResult> InContext<TResult>( Func<ApplicationDbContext, Task<TResult>> getter )
    {
      using var dbContext = await dbFactory.CreateDbContextAsync();
      return await getter( dbContext );
    }

    public class State
    {
      public ShowMessageData ShowMessageData { get; set; } = new();

      public List<string> SelectedCommandClients { get; set; } = [];

      public string? SelectedDBTablesTypeFullName { get; set; }
    }

    public record ClientInfo (string? remoteIpAddress, int remotePort, int localPort, string features);
  }
}
