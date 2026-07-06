using Toast.AndroidOS.Bootstrap;
using Toast.AndroidOS.Logging;
using Toast.AndroidOS.Notifications;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.AndroidOS.Services
{

  [Service(
    Enabled = true,
    Exported = false )]
  internal sealed class AgentService : Service
  {
    private CancellationTokenSource? _cts;
    private Task? _agentTask;

    private ILogger? _logger;

    public override void OnCreate()
    {
      base.OnCreate();

      _logger = CompositionRoot.CreateLogger();

      _logger.Info( this, "Created" );

      NotificationHelper.EnsureChannel( this );
    }

    public override StartCommandResult OnStartCommand(
        Intent? intent,
        StartCommandFlags flags,
        int startId )
    {
      _logger?.Info( this, "Started" );

      StartForeground(
        NotificationHelper.NotificationId,
        NotificationHelper.CreateNotification(
          this,
          AgentState.Starting ) );

      if ( _agentTask == null )
      {
        _cts = new CancellationTokenSource();

        var agent = CompositionRoot.Create( new AndroidStatusListener( this, _logger ) );

        _agentTask = Task.Run( () => agent.ExecuteAsync( _cts.Token ) );
      }

      return StartCommandResult.Sticky;
    }

    public override void OnDestroy()
    {
      _logger?.Info( this, "Service stopping" );

      _cts?.Cancel();

      base.OnDestroy();
    }

    public override IBinder? OnBind( Intent? intent )
    {
      return null;
    }

  }
}
