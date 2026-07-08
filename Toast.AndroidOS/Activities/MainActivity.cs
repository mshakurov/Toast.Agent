

using Java.Lang;

using Toast.AndroidOS.Bootstrap;
using Toast.AndroidOS.Services;
using Toast.Core.Interfaces;

namespace Toast.AndroidOS.Activities
{
  [Activity( Label = "@string/app_name", MainLauncher = true )]
  public class MainActivity : Activity
  {
    private TextView? textView;
    private readonly ILogger _logger = CompositionRoot.GetSingletonLogger();
    private PermissionCheckHelper? _permissionCheckHelper;
    private readonly CancellationTokenSource _ctsCheckPermissions = new();

    protected override void OnCreate( Bundle? savedInstanceState )
    {
      _logger.Info( this, $"OnCreate ({savedInstanceState?.KeySet()?.Count} keys)" );

      base.OnCreate( savedInstanceState );

      // Set our view from the "main" layout resource
      SetContentView( Resource.Layout.activity_main );

      SetupControls();

      _permissionCheckHelper = new PermissionCheckHelper( this, _logger, textView, _ctsCheckPermissions.Token );
    }

    protected override void OnStart()
    {
      base.OnStart();

      _permissionCheckHelper?.CheckPermissions();
    }

    protected override void OnStop()
    {
      _ctsCheckPermissions.Cancel();

      base.OnStop();
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
      SetupBtnStart();

      //  нопка Stop Ц останавливает сервис
      SetupBtnStop();

      //  нопка Exit Ц закрывает окно
      SetupBtnExit();

      //  нопка Settings Ц открывает окно настроек
      SetupBtnSettings();

      //  нопка Test1 Ц тест 1
      SetupBtnTest1();
    }

    private void SetupBtnSettings()
    {
      var btnSettings = FindViewById<Button>( Resource.Id.buttonSets );
      if ( btnSettings != null )
      {
        btnSettings.Click += ( sender, e ) =>
        {
          _logger.Info( this, $"buttonSets, Creating Intent" );
          var intent = new Intent( this, typeof( SettingsActivity ) );
          _logger.Info( this, $"buttonSets, Intent created. Calling StartActivity..." );
          StartActivity( intent );
          _logger.Info( this, $"buttonSets < StartActivity" );
        };
      }
      else
        _logger.Error( this, "# Button buttonSets not found" );
    }

    private void SetupBtnTest1()
    {
      var btnTest1 = FindViewById<Button>( Resource.Id.buttonTest1 );
      if ( btnTest1 != null )
      {
        btnTest1.Click += ( sender, e ) =>
        {
          btnTest1.Enabled = false;
          Task.Factory.StartNew( () =>
          {
            _logger.Info( this, $"buttonTest1, Creating test service ... " );

            var srv = CompositionRoot.CreateTestServerAuthorizedRequestService( _logger );

            _logger.Info( this, $"buttonTest1, Requesting test data..." );

            var result = srv.LoadItemsFromServerAsync().Result;

            this.RunOnUiThread( () =>
            {
              btnTest1.Enabled = true;

              if ( textView != null )
              {
                StringBuilder lines = new();
                lines.Append( $"Result ({result.Items.Count}):" + System.Environment.NewLine + string.Join( System.Environment.NewLine, result.Items.Select( r => $"- {r.Id}|{r.Name}|{r.Value}" ) ) );
                if ( !string.IsNullOrEmpty( result.Exception ) )
                  lines.Append( System.Environment.NewLine + $"Exception: {result.Exception}" );  
                textView.Text = lines.ToString();
              }
            } );
          } );
        };
      }
      else
        _logger.Error( this, "# Button buttonExit not found" );
    }

    private void SetupBtnExit()
    {
      var btnExit = FindViewById<Button>( Resource.Id.buttonExit );
      if ( btnExit != null )
      {
        btnExit.Click += ( sender, e ) =>
        {
          _logger.Info( this, $"btnExit > FinishAndRemoveTask ... " );

          FinishAndRemoveTask();

          _logger.Info( this, $"btnExit < FinishAndRemoveTask" );
        };
      }
      else
        _logger.Error( this, "# Button buttonExit not found" );
    }

    private void SetupBtnStop()
    {
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
    }

    private void SetupBtnStart()
    {
      var btnStart = FindViewById<Button>( Resource.Id.buttonStart );
      if ( btnStart != null )
      {
        btnStart.Click += ( sender, e ) =>
        {
          _permissionCheckHelper?.CheckPermissions();

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
    }
  }
}