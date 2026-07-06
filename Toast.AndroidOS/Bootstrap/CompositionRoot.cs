using Toast.AndroidOS.Logging;

using Toast.Core;
using Toast.Core.Interfaces;
using Toast.Core.Models;
using Toast.Core.Services;

namespace Toast.AndroidOS.Bootstrap;

internal static class CompositionRoot
{
  public static IAgent Create(IAgentStatusListener agentStatusListener)
  {
    ILogger logger = CreateLogger();

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


  public static ILogger CreateLogger()
  {
    return new AndroidLogger( GetSystemTag() );
  }

}