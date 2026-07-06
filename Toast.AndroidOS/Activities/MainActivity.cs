using Toast.AndroidOS.Bootstrap;
using Toast.AndroidOS.Logging;
using Toast.AndroidOS.Services;
using Toast.Core;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.AndroidOS.Activities
{
  [Activity( Label = "@string/app_name", MainLauncher = true )]
  public class MainActivity : Activity
  {
    private readonly ILogger _logger = AgentFactory.CreateLogger();

    protected override void OnCreate( Bundle? savedInstanceState )
    {
      _logger.Info( this, $"OnCreate ({savedInstanceState?.KeySet()?.Count} keys)" );

      base.OnCreate( savedInstanceState );

      // Set our view from the "main" layout resource
      SetContentView( Resource.Layout.activity_main );

      StartService(new Intent( this, typeof( AgentService ) ) );

      _logger.Info( this, $"OnCreate, Wait to Finish" );

      Task.Factory.StartNew( () =>
      {
        try
        {
          Task.Delay( TimeSpan.FromSeconds( 3 ) ).Wait();
        }
        catch 
        {
        }

        _logger.Info( this, $"OnCreate, > FinishAndRemoveTask ... " );

        FinishAndRemoveTask();

        _logger.Info( this, $"OnCreate, < FinishAndRemoveTask" );
      } );
    }
  }
}