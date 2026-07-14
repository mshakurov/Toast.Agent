using System.Runtime.Versioning;

using Android.Content.PM;

using Toast.AndroidOS.Bootstrap;
using Toast.AndroidOS.Notifications;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.AndroidOS.Services;


[Service(
  Enabled = true,
  Exported = false,
  ForegroundServiceType = ForegroundService.TypeDataSync )]
internal sealed class AgentService : Service
{
  private IBinder? _binder;
  private CancellationTokenSource? _cts;
  private Task? _agentTask;

  private ILogger? _logger;

  public override void OnCreate()
  {
    base.OnCreate();

    _logger = CompositionRoot.GetSingletonLogger();

    _logger.Info( this, "OnCreate" );

    NotificationHelper.EnsureChannel( this );

    _binder = new LocalBinder( this );
  }

  public override StartCommandResult OnStartCommand(
      Intent? intent,
      StartCommandFlags flags,
      int startId )
  {
    _logger?.Info( this, $"OnStartCommand" );

    _logger?.Debug( this, $"OnStartCommand, flags: {flags}, Intent: Extras?.KeySet: {intent?.Extras?.KeySet()}, Action: {intent?.Action}, Categories: [{string.Join( ", ", intent?.Categories ?? [] )}], DataString: {intent?.DataString}" );
    if ( OperatingSystem.IsAndroidVersionAtLeast( 29 ) )
      _logger?.Debug( this, $"OnStartCommand, Identifier: {intent?.Identifier}" );

    _logger?.Debug( this, "Starting Foreground..." );

    if ( OperatingSystem.IsAndroidVersionAtLeast( 29 ) )
      StartForeground29( AgentState.Starting );
    else
      StartForegroundBefore29( AgentState.Starting );

    _logger?.Debug( this, "Foreground started" );

    if ( _agentTask == null )
    {
      _logger?.Debug( this, "Service starting ..." );

      _cts = new CancellationTokenSource();

      var agent = CompositionRoot.CreateAgent( new AndroidStatusListener( this, _logger, _cts.Token ) );

      _agentTask = Task.Run( () => agent.ExecuteAsync( _cts.Token ) );

      //// Когда фоновая работа завершена, сервис завершает свой жизненный цикл сам
      //StopSelf();

      _logger?.Info( this, "Service started" );
    }

    return StartCommandResult.Sticky;
  }

  void StartForegroundBefore29( AgentState state )
  {
    StopForeground( StopForegroundFlags.Remove );
    StartForeground(
      NotificationHelper.NotificationId,
      NotificationHelper.CreateNotification(
        this,
        state ) );
  }

  [SupportedOSPlatform( "android29.0" )]
  void StartForeground29( AgentState state )
  {
    StopForeground( StopForegroundFlags.Remove );
    StartForeground(
      NotificationHelper.NotificationId,
      NotificationHelper.CreateNotification(
        this,
        state ),
      ForegroundService.TypeDataSync );
  }

  public override void OnDestroy()
  {
    _logger?.Info( this, "Service stopping" );

    _cts?.Cancel();

    base.OnDestroy();
  }

  public override IBinder? OnBind( Intent? intent )
  {
    return _binder;
  }

  public class LocalBinder( AgentService service ) : Binder
  {
    public AgentService Service { get; } = service;
  }
}
