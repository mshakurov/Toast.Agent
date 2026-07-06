using Android.App;
using Android.Content;
using Android.OS;

using Toast.AndroidOS.Logging;
using Toast.Core;
using Toast.Core.Interfaces;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toast.Core.Models;

namespace Toast.AndroidOS.Services
{

  [Service]
  public sealed class AgentService : Service
  {
    private CancellationTokenSource? _cts;
    private Task? _agentTask;

    private ILogger? _logger;

    public override void OnCreate()
    {
      base.OnCreate();

      _logger = new AndroidLogger( "AndroidOS" );

      _logger.Info( this, "Created" );
    }

    public override StartCommandResult OnStartCommand(
        Intent? intent,
        StartCommandFlags flags,
        int startId )
    {
      _logger?.Info( this, "Started" );

      if ( _agentTask == null )
      {
        _cts = new CancellationTokenSource();

        var context = new AgentContext
        {
          Logger = _logger!,
          Settings = new AgentSettings()
        };

        var agent = new Agent( context );

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
