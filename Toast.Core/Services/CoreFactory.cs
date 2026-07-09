using Toast.Core.Commands;
using Toast.Core.Interfaces;
using Toast.Core.Models;
using Toast.Core.Networking;

namespace Toast.Core.Services
{
  public static class CoreFactory
  {

    public static IAgentService CreateAgentService( ILogger logger, IHostSettings settings, IHostStatusListener agentStatusListener, IHostShowMessage hostShowMessage )
    {
      var agentContext = new HostingContext
      {
        Logger = logger,
        Settings = settings,
        AgentStatusListener = agentStatusListener,
        HostShowMessage = hostShowMessage
      };

      return new AgentService( agentContext );
    }

    internal static IPollingService CreatePollingService( HostingContext context ) => new PollingService( context );


    public static ITestServerAuthorizedRequestService CreateTestServerAuthorizedRequestService( string baseServerUrl, LoginModel credentials, ILogger logger ) => new TestServerAuthorizedRequestService( new SecureClient( baseServerUrl, credentials, null, logger ).SecureDataClient, logger );
  }
}
