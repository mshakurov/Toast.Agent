using Java.Lang;

using Toast.AndroidOS.Bootstrap;
using Toast.AndroidOS.Models;
using Toast.AndroidOS.Services;
using Toast.Core.Utilities;
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
    
    internal AgentService? AgentService;
    private AgentServiceConnection? _serviceConnection;

    protected override void OnCreate( Bundle? savedInstanceState )
    {
      _logger.Info( this, $"OnCreate ({savedInstanceState?.KeySet()?.Count} keys)" );

      base.OnCreate( savedInstanceState );

      // Set our view from the "main" layout resource
      SetContentView( Resource.Layout.activity_main );

      SetupControls();

      _permissionCheckHelper = new PermissionCheckHelper( this, _logger, FindViewById<TextView>( Resource.Id.perm_text ), _ctsCheckPermissions.Token );
    }

    protected override void OnStart()
    {
      base.OnStart();

      _permissionCheckHelper?.StartCheckPermissions();

      // Начинаем подключение к сервису при каждом открытии приложения
      _logger.Info( this, $"OnStart > BindService (AgentService: {AgentService != null})" );
      BindToAgentService();
      _logger.Info( this, $"OnStart < BindService (AgentService: {AgentService != null})" );
    }

    protected override void OnStop()
    {
      base.OnStop();

      _ctsCheckPermissions.Cancel();

      // ОБЯЗАТЕЛЬНО отвязываемся при закрытии экрана, чтобы избежать утечек памяти
      _logger.Info( this, $"OnStop > BindService (AgentService: {AgentService != null})" );
      UnbindFromAgentService();
      _logger.Info( this, $"OnStop < BindService (AgentService: {AgentService != null})" );
    }

    private void BindToAgentService()
    {
      _serviceConnection = new AgentServiceConnection( this );
      Intent intent = new Intent( this, typeof( AgentService ) );
      BindService( intent, _serviceConnection, Bind.None );
    }

    void UnbindFromAgentService()
    {
      if ( AgentService != null && _serviceConnection != null )
      {
        _logger.Info( this, $"OnStart > UnbindService (AgentService: {AgentService != null})" );
        UnbindService( _serviceConnection );
        _logger.Info( this, $"OnStart < UnbindService (AgentService: {AgentService != null})" );
      }
    }

    // Метод вызывается автоматически из MyServiceConnection, когда связь установлена
    public void OnServiceSuccessfullyConnected()
    {
      RunOnUiThread( () => {
        _logger.Info( this, "! OnServiceSuccessfullyConnected" );

        Android.Widget.Toast.MakeText( this, "Успешно подключено к фоновому сервису!", ToastLength.Short )?.Show();

        this.PrependLine( textView, "!!! Подключено !!!" );
      } );
    }

    public void OnServiceDisconnected()
    {
      RunOnUiThread( () => {
        UnbindFromAgentService();

        _logger.Info( this, "@ OnServiceDisconnected" );

        Android.Widget.Toast.MakeText( this, "Остановлен фоновый сервис!", ToastLength.Short )?.Show();

        this.PrependLine( textView, "@@@ Отключено @@@" );
      } );
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

          new ShowMessageService( _logger ).StartShowMessage( "Тестовое сообщение\nс переносом строки", "♥♥♥", 10, log =>
          {
            _logger.Info( this, $"btnTestShowMessageService: onResult('{log}')" );
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

            var settings = CompositionRoot.GetSingletonSettingsService().LoadSettings();
            var servers = settings.GetValidServers().Select( ( s, i ) => (s, i: (ushort)i) ).OrderBy( s => s.i == settings.LastSuccessfulServerIndex ? 1 : 2 ).ThenBy( s => s.i ).ToArray();
            if ( servers.Length == 0 )
            {
              this.RunOnUiThread( () =>
              {
                if ( textView != null )
                {
                  textView.Text = $"# Не найден ни один правильно настроенный сервер";
                }
              } );
            }
            else
            {
              foreach ( var server in servers )
              {
                try
                {
                  this.PrependLine( textView, $"Тестируем {server} ...", "-----" );

                  var srv = CompositionRoot.CreateTestServerAuthorizedRequestService( server.s.BaseUrl, server.s.LoginModel!, _logger );

                  _logger.Info( this, $"buttonTestRequest, Requesting test data {server}..." );

                  (List<Core.Commands.TestDataItem> Items, string? Exception) result = srv.LoadItemsFromServerAsync().Result;

                  _logger.Info( this, $"buttonTestRequest, Received test data result {server}: Item count: {result.Items.Count}, HasExcept: {result.Exception != null}" );

                  StringBuilder lines = new();
                  lines.Append( $"Server: {server.s.GetKey()}." );
                  lines.Append( System.Environment.NewLine + $"Result ({result.Items.Count}):" + System.Environment.NewLine + string.Join( System.Environment.NewLine, result.Items.Select( r => $"- {r.Id}|{r.Name}|{r.Value}" ) ) );
                  if ( !string.IsNullOrEmpty( result.Exception ) )
                    lines.Append( System.Environment.NewLine + $"Exception: {result.Exception}" );
                  var text = $"Сервер {server} вернул: {lines.ToString() ?? "[ничего]"}";
                  this.PrependLine( textView, text, "-----" );
                  _logger.Info( this, $"# buttonTestRequest, Requesting test data result: {text}" );

                  if ( result.Exception == null )
                  {
                    if ( server.i != settings.LastSuccessfulServerIndex )
                    {
                      settings.LastSuccessfulServerIndex = server.i;
                      settings.Update();
                    }
                    break;
                  }
                }
                catch ( System.Exception ex )
                {
                  var text = $"Exception: {ex.GetFullMessage()}";
                  _logger.Error( this, $"# buttonTestRequest, Requesting test data error: {text}" );
                  this.PrependLine( textView, text, "-----" );
                }
              }

              this.RunOnUiThread( () =>
              {
                btnTestRequest.Enabled = true;
              } );
              this.PrependLine( textView, "Тесты завершены", "-----" );
            }
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
          _permissionCheckHelper?.StartCheckPermissions();

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