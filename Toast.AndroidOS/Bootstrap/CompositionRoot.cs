using Toast.AndroidOS.Logging;

using Toast.Core;
using Toast.Core.Interfaces;
using Toast.Core.Models;
using Toast.Core.Services;

namespace Toast.AndroidOS.Bootstrap;

internal static class CompositionRoot
{
  static ILogger? _singletonLogger;

  public static IAgent CreateAgent(IAgentStatusListener agentStatusListener)
  {
    ILogger logger = GetSingletonLogger();

    var context = new AgentContext
    {
      Logger = logger,
      Settings = new AgentSettings(),
      AgentStatusListener = agentStatusListener,
      PollingService = CoreFactory.CreatePollingService(logger )
    };

    return CoreFactory.CreateAgent( context );
  }

  public static string GetSystemTag() => nameof( Toast.AndroidOS ).Split( '.' ).Last();


  public static ILogger CreateNewLogger() => new AndroidLogger( GetSystemTag() );

  public static ILogger GetSingletonLogger() => _singletonLogger ??= new AndroidLogger( GetSystemTag() );

  public static ITestServerAuthorizedRequestService CreateTestServerAuthorizedRequestService( ILogger logger ) => CoreFactory.CreateTestServerAuthorizedRequestService( GetSingletonLogger() );

}