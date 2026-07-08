
using Toast.AndroidOS.Logging;
using Toast.AndroidOS.Models;
using Toast.AndroidOS.Services;
using Toast.Core.Interfaces;
using Toast.Core.Services;

namespace Toast.AndroidOS.Bootstrap;

internal static class CompositionRoot
{
  static ILogger? _singletonLogger;
  static SettingsService? _singletonSettingsService;

  public static string PackageName => Application.Context.PackageName ?? "com.maratsh.Toast.AndroidOS";

  public static IAgentService CreateAgent( IHostStatusListener agentStatusListener ) => CoreFactory.CreateAgentService( GetSingletonLogger(), new HostSettings(), agentStatusListener );

  public static string GetSystemTag() => nameof( Toast.AndroidOS ).Split( '.' ).Last();


  public static ILogger CreateNewLogger() => new AndroidLogger( GetSystemTag() );

  public static ILogger GetSingletonLogger() => _singletonLogger ??= new AndroidLogger( GetSystemTag() );

  public static ITestServerAuthorizedRequestService CreateTestServerAuthorizedRequestService( ILogger logger ) => CoreFactory.CreateTestServerAuthorizedRequestService( GetSingletonLogger() );

  public static SettingsService GetSingletonSettingsService() => _singletonSettingsService ??= new SettingsService( PackageName, GetSingletonLogger() );

}