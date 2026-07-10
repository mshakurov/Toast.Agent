using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Toast.Core.Commands;
using Toast.Core.Commands.CommandData;
using Toast.Server.Data.Models;

namespace Toast.Server.Data
{
  public class CommandService
  {
    private readonly ApplicationDbContext dbContext;

    public readonly State Current = new State();

    public CommandService( ApplicationDbContext dbContext )
    {
      this.dbContext = dbContext;
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

    public async Task<AgentResponse> GetCommands( AgentRequest request, CancellationToken token = default )
    {
      //List<AgentCommand> commands =
      //  [
      //    new () { Id = Guid.NewGuid(), Type = CommandTypes.ShowMessage,  JsonParameters = JsonSerializer.Serialize( new ShowMessageData { Title = "Hallow device!", Message  = $"From server! You are: {request.AgentId}", Duration = 11, WaitIfShow = false  } ) }
      //  ];

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
          dbContext.RemoveRange( commandsFor );
          _ = dbContext.SaveChangesAsync( token );
        }
        return new AgentResponse { Commands = [.. commandsFor.Select( cf => cf.Command )] };
      }
    }

    public async Task<AgentClient[]> GetAllAgentClients( CancellationToken token = default )
      => await dbContext.AgentClient.ToArrayAsync( token );

    public async Task<List<AgentCommandFor>> SendAsync( List<string> selectedCommandClients, ShowMessageData showMessageData, CancellationToken? token = default )
    {
      List<AgentCommandFor> added = new( selectedCommandClients.Count );

      foreach ( var clientID in selectedCommandClients )
        added.Add( dbContext.AgentCommandFor.Add( new AgentCommandFor() { ClientId = clientID, Command = new AgentCommand() { Id = Guid.NewGuid(), Type = CommandTypes.ShowMessage, JsonParameters = JsonSerializer.Serialize( showMessageData ) } } ).Entity );

      await dbContext.SaveChangesAsync( token ?? CancellationToken.None );

      return added;
    }

    public class State
    {
      public ShowMessageData ShowMessageData { get; set; } = new();

      public List<string> SelectedCommandClients { get; set; } = [];
    }
  }
}
