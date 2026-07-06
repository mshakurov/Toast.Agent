using Toast.AndroidOS.Logging;

using Toast.Core;
using Toast.Core.Interfaces;
using Toast.Core.Models;
using Toast.Core.Services;

namespace Toast.AndroidOS.Bootstrap;

internal static class AgentFactory
{
  public static Agent Create()
  {
    ILogger logger = CreateLogger();

    var context = new AgentContext
    {
      Logger = logger,
      Settings = new AgentSettings()
    };

    return new Agent( context );
  }

  public static string GetSystemTag() => nameof( Toast.AndroidOS ).Split( '.' ).Last();


  public static ILogger CreateLogger()
  {
    return new AndroidLogger( GetSystemTag() );
  }

  public static IPollingService CreatePollingService( ILogger logger ) => CoreFactory.CreatePollingService( logger );

}