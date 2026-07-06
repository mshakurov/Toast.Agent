using Toast.Core.Interfaces;
using Toast.Core.Models;
using Toast.Core.Networking;

namespace Toast.Core.Services
{
  public static class CoreFactory
  {

    public static IAgent CreateAgent( AgentContext context ) => new Agent( context );

    public static IPollingService CreatePollingService( ILogger logger ) => new PollingService( logger );

    public static ITestServerAuthorizedRequestService CreateTestServerAuthorizedRequestService( ILogger logger ) => new TestServerAuthorizedRequestService( new SecureClient( logger ).SecureDataClient, logger );
  }
}
