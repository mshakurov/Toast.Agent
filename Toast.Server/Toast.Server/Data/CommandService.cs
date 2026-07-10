using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Toast.Core.Commands;
using Toast.Core.Commands.CommandData;
using Toast.Server.Data.Models;

namespace Toast.Server.Data
{
  public class CommandService
  {
    public readonly State Current = new State();
    private readonly IDbContextFactory<ApplicationDbContext> dbFactory;

    public CommandService( IDbContextFactory<ApplicationDbContext> dbFactory )
    {
      this.dbFactory = dbFactory;
    }

    public List<TestDataItem> GetProtectedData( params TestDataItem[] addDefaultItems )
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

    public async Task<TResult> InContext<TResult>( Func<ApplicationDbContext, Task<TResult>> getter )
    {
      using var dbContext = await dbFactory.CreateDbContextAsync();
      return await getter( dbContext );
    }

    public async Task<AgentResponse> GetCommands( AgentRequest request, CancellationToken token = default )
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
        return new AgentResponse();
      }
      else
      {
        var commandsFor = await dbContext.AgentCommandFor.Include( c => c.Client ).Include( c => c.Command ).Where( ac => ac.Client.ClientId == request.AgentId ).ToListAsync( token );
        if ( commandsFor.Count > 0 )
        {
          try
          {
            dbContext.RemoveRange( commandsFor );
            //_ = Task.Run( async () =>
            //{
              try
              {
                await dbContext.SaveChangesAsync( token );
              }
              catch ( Exception exSave )
              {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine( $"### Ошибка сохранения удалений AgentCommandFor после отправки команд: {exSave}" );
                Console.ResetColor();
              }
            //} );
          }
          catch ( Exception ex )
          {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine( $"### Ошибка удаления AgentCommandFor после отправки команд: {ex}" );
            Console.ResetColor();
          }
        }
        return new AgentResponse { Commands = [.. commandsFor.Select( cf => cf.Command )] };
      }
    }

    public async Task<AgentClient[]> GetAllAgentClients( CancellationToken token = default )
      => await InContext( async dbContext => await dbContext.AgentClient.ToArrayAsync( token ) );

    public async Task<List<AgentCommandFor>> SendAsync( List<string> selectedCommandClients, ShowMessageData showMessageData, CancellationToken? token = default )
    {
      List<AgentCommandFor> added = new( selectedCommandClients.Count );

      using var dbContext = await dbFactory.CreateDbContextAsync();

      foreach ( var clientID in selectedCommandClients )
        added.Add( dbContext.AgentCommandFor.Add( new AgentCommandFor() { ClientId = clientID, Command = new AgentCommand() { Id = Guid.NewGuid(), Type = CommandTypes.ShowMessage, JsonParameters = JsonSerializer.Serialize( showMessageData ) } } ).Entity );

      await dbContext.SaveChangesAsync( token ?? CancellationToken.None );

      return added;
    }

    public class State
    {
      public ShowMessageData ShowMessageData { get; set; } = new();

      public List<string> SelectedCommandClients { get; set; } = [];

      public string? SelectedDBTablesTypeFullName { get; set; }
    }
  }
}
