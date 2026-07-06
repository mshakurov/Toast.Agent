using Toast.Core.Commands;
using Toast.Core.Interfaces;
using Toast.Core.Models;
using Toast.Core.Networking;

namespace Toast.Core.Services
{
  public static class CoreFactory
  {

    public static IAgent CreateAgent( ILogger logger, AgentSettings settings, IAgentStatusListener agentStatusListener )
    {
      var context = new AgentContext
      {
        Logger = logger,
        Settings = settings,
        AgentStatusListener = agentStatusListener,
        CommandHandlers = CommandHandlerFactory.Create(),
        CommandProvider = new HttpCommandProvider(),
        CommandReporter = new HttpCommandReporter(),
      };

      return new Agent( context );
    }

    public static IPollingService CreatePollingService( AgentContext context ) => new PollingService( context );

    public static ITestServerAuthorizedRequestService CreateTestServerAuthorizedRequestService( ILogger logger ) => new TestServerAuthorizedRequestService( new SecureClient( logger ).SecureDataClient, logger );
  }
}
