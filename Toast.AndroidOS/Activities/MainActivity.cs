



using Java.Lang;

using Toast.AndroidOS.Bootstrap;
using Toast.AndroidOS.Models;
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

      // Кнопка Start – запускает сервис
      SetupBtnStart();

      // Кнопка Stop – останавливает сервис
      SetupBtnStop();

      // Кнопка Exit – закрывает окно
      SetupBtnExit();

      // Кнопка Settings – открывает окно настроек
      SetupBtnSettings();

      // Кнопка Test Request – тест запроса
      SetupBtnTestRequest();

      // Кнопка Test Show Message – тест показа сообщения
      SetupBtnTestShowMessage();

      // Кнопка тестирования сервис показа сообщений
      SetupBtnTestShowMsgService();
    }

    private void SetupBtnTestShowMsgService()
    {
      var btnTestShowMessageService = FindViewById<Button>( Resource.Id.buttonTestShowMessageService );
      if ( btnTestShowMessageService != null )
      {
        btnTestShowMessageService.Click += ( sender, e ) =>
        {
          _logger.Info( this, $"btnTestShowMessageService, Creating Intent" );

          new ShowMessageService(_logger).StartShowMessage( "Тестовое сообщение\nс переносом строки", "♥♥♥", 10, exception =>
          {
            _logger.Info( this, $"btnTestShowMessageService: onResult('{exception}')" );
          } );

          _logger.Info( this, $"btnTestShowMessageService < StartActivity" );
        };
      }
      else
        _logger.Error( this, "# Button btnTestShowMessageService not found" );
    }

    private void SetupBtnTestShowMessage()
    {
      var btnTestShowMessage = FindViewById<Button>( Resource.Id.buttonTestShowMessage );
      if ( btnTestShowMessage != null )
      {
        btnTestShowMessage.Click += ( sender, e ) =>
        {
          _logger.Info( this, $"buttonTestShowMessage, Creating Intent" );
          var intent = new Intent( this, typeof( ShowMessageActivity ) );
          intent.PutExtra( ShowMessageActivity.C_IntentExtraText, "Тестовое сообщение\nс переносом строки" );
          intent.PutExtra( ShowMessageActivity.C_IntentExtraDuration, 10 );
          intent.PutExtra( ShowMessageActivity.C_IntentExtraTitle, "♥♥♥" );
          _logger.Info( this, $"buttonTestShowMessage, Intent created. Calling StartActivity..." );
          StartActivity( intent );
          _logger.Info( this, $"buttonTestShowMessage < StartActivity" );
        };
      }
      else
        _logger.Error( this, "# Button buttonTestShowMessage not found" );
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

    private void SetupBtnTestRequest()
    {
      var btnTestRequest = FindViewById<Button>( Resource.Id.buttonTestRequest );
      if ( btnTestRequest != null )
      {
        btnTestRequest.Click += ( sender, e ) =>
        {
          btnTestRequest.Enabled = false;
          Task.Factory.StartNew( () =>
          {
            _logger.Info( this, $"buttonTestRequest, Creating test service ... " );

            string text;

            var servers = new HostSettings().GetValidServers();
            if ( servers.Length == 0 )
              text = $"# Не найден ни один правильно настроенный сервер";
            else
            {
              try
              {
                var server = servers.Last();

                var srv = CompositionRoot.CreateTestServerAuthorizedRequestService( server.BaseUrl, server.LoginModel!, _logger );

                _logger.Info( this, $"buttonTestRequest, Requesting test data..." );
                
                var result = srv.LoadItemsFromServerAsync().Result;
                
                StringBuilder lines = new();
                lines.Append( $"Server: {server.GetKey()}." );
                lines.Append( System.Environment.NewLine + $"Result ({result.Items.Count}):" + System.Environment.NewLine + string.Join( System.Environment.NewLine, result.Items.Select( r => $"- {r.Id}|{r.Name}|{r.Value}" ) ) );
                if ( !string.IsNullOrEmpty( result.Exception ) )
                  lines.Append( System.Environment.NewLine + $"Exception: {result.Exception}" );
                text = lines.ToString() ?? string.Empty;
              }
              catch ( System.Exception ex )
              {
                text = $"Exception: {ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}";
              }
            }

            _logger.Error( this, text );

            this.RunOnUiThread( () =>
            {
              btnTestRequest.Enabled = true;

              if ( textView != null )
              {
                textView.Text = text;
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