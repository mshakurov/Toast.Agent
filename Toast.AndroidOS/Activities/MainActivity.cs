using Toast.AndroidOS.Logging;
using Toast.Core.Interfaces;

namespace Toast.AndroidOS.Activities
{
  [Activity( Label = "@string/app_name", MainLauncher = true )]
  public class MainActivity : Activity
  {
    private readonly ILogger _logger = new AndroidLogger();

    protected override void OnCreate( Bundle? savedInstanceState )
    {
      _logger.Info( this, $"OnCreate ({savedInstanceState?.KeySet()?.Count} keys)" );

      savedInstanceState?.KeySet()?.ToList().ForEach( key =>
      {
        var value = savedInstanceState.GetString( key );
        _logger.Info( this, $"SavedInstanceState: {key} = {value}" );
      } );

      base.OnCreate( savedInstanceState );

      // Set our view from the "main" layout resource
      SetContentView( Resource.Layout.activity_main );
    }
  }
}