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
    private TextView? textView;
    private readonly ILogger _logger = CompositionRoot.CreateLogger();
    private int requestPermissionCount = 0;

    protected override void OnCreate( Bundle? savedInstanceState )
    {
      _logger.Info( this, $"OnCreate ({savedInstanceState?.KeySet()?.Count} keys)" );

      base.OnCreate( savedInstanceState );

      // Set our view from the "main" layout resource
      SetContentView( Resource.Layout.activity_main );

      SetupControls();

      CheckPermissions();
    }

    protected override void OnStart()
    {
      base.OnStart();
    }

    private bool CheckPermissions()
    {
      if ( OperatingSystem.IsAndroidVersionAtLeast( 33 ) )
      {
        var granted =
            CheckSelfPermission( Android.Manifest.Permission.PostNotifications );

        Android.Util.Log.Info(
            "Toast",
            $"POST_NOTIFICATIONS = {granted}" );

        if ( textView != null )
          textView.Text = $"POST_NOTIFICATIONS = {granted}";

        if ( granted != Android.Content.PM.Permission.Granted )
        {
          if ( ++requestPermissionCount <= 2 )
          {
            RequestPermissions(
                new[]
                {
                  Android.Manifest.Permission.PostNotifications
                },
                100 );
          }
          else
          {
            Android.Widget.Toast.MakeText(this, Resource.String.setNotificationPermissionsEn, ToastLength.Long)?.Show();

            var intent = new Intent( Android.Provider.Settings.ActionApplicationDetailsSettings );
            intent.SetData( Android.Net.Uri.Parse( $"package:{PackageName}" ) );
            StartActivity( intent );
          }
        }

        return CheckSelfPermission( Android.Manifest.Permission.PostNotifications ) == Android.Content.PM.Permission.Granted;
      }
      return true;
    }

    public override bool ShouldShowRequestPermissionRationale( string permission )
    {
      _logger.Debug( this, $"ShouldShowRequestPermissionRationale('{permission}')" );
      return true;
      //return base.ShouldShowRequestPermissionRationale( permission );
    }

    public override bool ShouldShowRequestPermissionRationale( string permission, int deviceId )
    {
      _logger.Debug( this, $"ShouldShowRequestPermissionRationale('{permission}, {deviceId}')" );
      //if ( OperatingSystem.IsAndroidVersionAtLeast( 35 ) )
      //  return base.ShouldShowRequestPermissionRationale( permission, deviceId );
      return true;
    }

    void SetupControls()
    {
      textView = FindViewById<TextView>( Resource.Id.app_text );

      //  нопка Start Ц запускает сервис
      var btnStart = FindViewById<Button>( Resource.Id.buttonStart );
      if ( btnStart != null )
      {
        btnStart.Click += ( sender, e ) =>
        {
          CheckPermissions();

          _logger.Debug( this, $"buttonStart, Creating Intent" );

          var intent = new Intent( this, typeof( AgentService ) );

          _logger.Debug( this, $"buttonStart, Intent created. Calling StartService..." );

          StartService( intent );

          _logger.Debug( this, $"buttonStart, StartService call finished." );
        };

        btnStart.RequestFocus();
        btnStart.Post( () => btnStart.RequestFocus() );
        btnStart.PostDelayed( () => btnStart.RequestFocus(), 100 );
      }
      else
        _logger.Error( this, "# Button buttonStart not found" );

      //  нопка Stop Ц останавливает сервис
      var btnStop = FindViewById<Button>( Resource.Id.buttonStop );
      if ( btnStop != null )
      {
        btnStop.Click += ( sender, e ) =>
        {
          _logger.Debug( this, $"buttonStop, Creating Intent" );

          var intent = new Intent( this, typeof( AgentService ) );

          _logger.Debug( this, $"buttonStop, Intent created. Calling StopService..." );

          StopService( intent );

          _logger.Debug( this, $"buttonStop, StopService call finished." );
        };

        btnStop.RequestFocus();
        btnStop.Post( () => btnStop.RequestFocus() );
        btnStop.PostDelayed( () => btnStop.RequestFocus(), 100 );
      }
      else
        _logger.Error( this, "# Button buttonStop not found" );

      //  нопка Exit Ц закрывает окно
      var btnExit = FindViewById<Button>( Resource.Id.buttonExit );
      if ( btnExit != null )
      {
        btnExit.Click += ( sender, e ) =>
        {
          _logger.Info( this, $"OnCreate, > FinishAndRemoveTask ... " );

          FinishAndRemoveTask();

          _logger.Info( this, $"OnCreate, < FinishAndRemoveTask" );
        };
      }
      else
        _logger.Error( this, "# Button buttonExit not found" );

    }
  }
}