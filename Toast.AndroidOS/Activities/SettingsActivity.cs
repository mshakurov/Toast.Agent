using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using Toast.AndroidOS.Bootstrap;
using Toast.Core.Commands;
using Toast.AndroidOS.Models;
using System.Diagnostics.CodeAnalysis;
using Toast.AndroidOS.Services;
using Toast.Core.Utilities;


namespace Toast.AndroidOS.Activities
{

  [Activity( Label = "Настройки", MainLauncher = false )]
  public class SettingsActivity : Activity
  {
    private EditText? _editPollingInterval;
    private TextView? _editUID;
    private TextView? _editHARDWARE;
    private LinearLayout? _serversContainer;
    private Button? _btnSave;
    private Button? _btnReload;
    private Button? _btnClose;
    private Button? _btnAddServer;

    // Храним ссылки на EditText поля для серверов, чтобы прочитать их при сохранении
    private List<ServerInputFields> _serverFieldsList = new();

    protected override void OnCreate( Bundle? savedInstanceState )
    {
      base.OnCreate( savedInstanceState );
      SetContentView( Resource.Layout.activity_settings );

      try
      {
        FindControls();
      }
      catch ( Exception ex )
      {
        Android.Widget.Toast.MakeText( this, $"Не все компоненты найдены: {ex.GetFullMessage()}", ToastLength.Short )?.Show();
        return;
      }

      // Привязка событий к трем кнопкам
      _btnSave.Click += OnSaveClicked;
      _btnReload.Click += OnReloadClicked;
      _btnClose.Click += OnCloseClicked;
      _btnAddServer.Click += OnAddServerClicked; // Обработчик добавления

      // Первоначальная загрузка данных
      LoadSettingsIntoUI();
    }

    [MemberNotNull( nameof( _editPollingInterval ), nameof( _serversContainer ), nameof( _btnSave ), nameof( _btnReload ), nameof( _btnClose ), nameof( _btnAddServer ), nameof( _editUID ), nameof( _editHARDWARE ) )]
    private void FindControls()
    {
      // Инициализация элементов UI
      _editPollingInterval = FindViewById<EditText>( Resource.Id.editPollingInterval ) ?? throw new KeyNotFoundException( "EditText for polling interval not found" );
      _editUID = FindViewById<TextView>( Resource.Id.editUID ) ?? throw new KeyNotFoundException( "EditText for UID not found" );
      _editHARDWARE = FindViewById<TextView>( Resource.Id.editHARDWARE ) ?? throw new KeyNotFoundException( "EditText for HARDWARE not found" );
      _serversContainer = FindViewById<LinearLayout>( Resource.Id.serversContainer ) ?? throw new KeyNotFoundException( "LinearLayout for servers container not found" );
      _btnSave = FindViewById<Button>( Resource.Id.btnSave ) ?? throw new KeyNotFoundException( "Button for save not found" );
      _btnReload = FindViewById<Button>( Resource.Id.btnReload ) ?? throw new KeyNotFoundException( "Button for reload not found" );
      _btnClose = FindViewById<Button>( Resource.Id.btnClose ) ?? throw new KeyNotFoundException( "Button for close not found" );
      _btnAddServer = FindViewById<Button>( Resource.Id.btnAddServer ) ?? throw new KeyNotFoundException( "Button for add server not found" );
    }

    private void LoadSettingsIntoUI()
    {
      // Очищаем контейнер серверов и кэш полей перед (пере)загрузкой
      _serversContainer!.RemoveAllViews();
      _serverFieldsList.Clear();

      // 1. Читаем настройки через ваш CompositionRoot
      // (Предполагается, что ReadSettings возвращает или заполняет объект. 
      // Если метод ничего не возвращает, передаем туда пустой инстанс для заполнения)
      var settings = CompositionRoot.GetSingletonSettingsService().LoadSettings();

      // 2. Заполняем поле интервала опроса
      _editPollingInterval!.Text = settings.PollingInterval.ToString();

      _editUID!.Text = settings.HostUID;

      _editHARDWARE!.Text = DeviceHardwareProviderService.GetDeviceHumanReadableName();

      // 3. Динамически строим форму для каждого сервера из массива
      if ( settings.Servers != null )
      {
        foreach ( var server in settings.Servers )
        {
          AddServerToUI( server );
        }
      }
    }

    private void AddServerToUI( RemoteServer server )
    {
      // Корневой контейнер для одного сервера
      var serverBox = new LinearLayout( this )
      {
        Orientation = Orientation.Vertical,
        LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent )
      };
      serverBox.SetPadding( 0, 0, 0, 24 );

      // Заголовок блока сервера и кнопка "Удалить"
      var headerLayout = new RelativeLayout( this );
      var txtServerHeader = new TextView( this ) { Text = $"Конфигурация сервера:", Typeface = Android.Graphics.Typeface.DefaultBold };
      txtServerHeader.SetTextColor( Android.Graphics.Color.ParseColor( "#555555" ) );

      var btnDelete = new Button( this )
      {
        Text = "Удалить",
        TextSize = 11
      };
      btnDelete.SetBackgroundColor( Android.Graphics.Color.OrangeRed );
      btnDelete.SetTextColor( Android.Graphics.Color.White );

      var deleteParams = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
      deleteParams.AddRule( LayoutRules.AlignParentEnd );
      btnDelete.LayoutParameters = deleteParams;

      headerLayout.AddView( txtServerHeader );
      headerLayout.AddView( btnDelete );
      serverBox.AddView( headerLayout );

      // Поля ввода
      var editUrl = new EditText( this ) { Hint = "URL (http://host:port)", Text = server.HostURL };
      editUrl.SetTextColor( Android.Graphics.Color.Black );

      var editPort = new EditText( this ) { Hint = "Порт (например, 7101)", Text = server.Port.ToString(), InputType = Android.Text.InputTypes.ClassNumber };
      editPort.SetTextColor( Android.Graphics.Color.Black );

      var editPath = new EditText( this ) { Hint = "API Base Path", Text = server.APIBasePath };
      editPath.SetTextColor( Android.Graphics.Color.Black );

      var editEmail = new EditText( this ) { Hint = "Email / Логин", Text = server.LoginModel?.Email ?? "", InputType = Android.Text.InputTypes.TextVariationEmailAddress };
      editEmail.SetTextColor( Android.Graphics.Color.Black );

      // Поле пароля (скрытый ввод точек)
      var editPassword = new EditText( this ) { Hint = "Пароль", Text = server.LoginModel?.Password ?? "", InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword };
      editPassword.SetTextColor( Android.Graphics.Color.Black );

      // Собираем элементы в контейнер
      serverBox.AddView( editUrl );
      serverBox.AddView( editPort );
      serverBox.AddView( editPath );
      serverBox.AddView( editEmail );
      serverBox.AddView( editPassword );

      // Разделительная линия
      var separator = new View( this )
      {
        LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 3 ),
        Background = new Android.Graphics.Drawables.ColorDrawable( Android.Graphics.Color.Gray )
      };
      serverBox.AddView( separator );

      _serversContainer!.AddView( serverBox );

      // Структура для сохранения ссылок
      var fieldsMapping = new ServerInputFields
      {
        EditUrl = editUrl,
        EditPort = editPort,
        EditPath = editPath,
        EditEmail = editEmail,
        EditPassword = editPassword,
        OriginalServerReference = server
      };
      _serverFieldsList.Add( fieldsMapping );

      // Логика кнопки "Удалить"
      btnDelete.Click += ( s, e ) =>
      {
        _serversContainer.RemoveView( serverBox ); // Удаляем визуально с экрана
        _serverFieldsList.Remove( fieldsMapping ); // Удаляем из внутренней коллекции данных
      };
    }

    private void OnAddServerClicked( object? sender, EventArgs e )
    {
      // Добавляем пустой сервер с дефолтными значениями из класса
      AddServerToUI( new RemoteServer() );
    }

    private void OnSaveClicked( object? sender, EventArgs e )
    {
      var updatedSettings = new HostSettings();

      // 1. Считываем интервал опроса
      if ( ushort.TryParse( _editPollingInterval!.Text, out ushort interval ) )
      {
        updatedSettings.PollingInterval = interval;
      }

      // 2. Собираем массив серверов из динамических полей UI
      var updatedServers = new List<RemoteServer>();

      foreach ( var fields in _serverFieldsList )
      {
        int.TryParse( fields.EditPort.Text, out int port );

        var server = new RemoteServer
        {
          HostURL = fields.EditUrl.Text,
          Port = port == 0 ? 7101 : port,
          APIBasePath = fields.EditPath.Text ?? RemoteServer.C_APIBasePath,
          // Переносим старый токен авторизации, чтобы он не затерся при пересохранении
          LastAuthToken = fields.OriginalServerReference?.LastAuthToken
        };

        // Если логин или пароль заполнены, создаем LoginModel
        if ( !string.IsNullOrWhiteSpace( fields.EditEmail.Text ) || !string.IsNullOrWhiteSpace( fields.EditPassword.Text ) )
        {
          server.LoginModel = new LoginModel( fields.EditEmail.Text ?? string.Empty, fields.EditPassword.Text ?? string.Empty );
        }

        updatedServers.Add( server );
      }

      updatedSettings.Servers = updatedServers.ToArray();

      // 3. Вызываем ваш метод сохранения
      CompositionRoot.GetSingletonSettingsService().SaveSettings( updatedSettings );

      Android.Widget.Toast.MakeText( this, "Настройки успешно сохранены", ToastLength.Short )?.Show();
    }

    private void OnReloadClicked( object? sender, EventArgs e )
    {
      // Перечитываем данные из хранилища, заменяя все изменения на экране
      LoadSettingsIntoUI();
      Android.Widget.Toast.MakeText( this, "Данные перезагружены", ToastLength.Short )?.Show();
    }

    private void OnCloseClicked( object? sender, EventArgs e )
    {
      // Закрываем текущую Activity и возвращаемся на предыдущий экран
      Finish();
    }

    // Добавьте этот код внутрь класса SettingsActivity
    public override void OnBackPressed()
    {
      // Проверяем: если клавиатура/панель ввода активна, мы можем попробовать её принудительно скрыть
      var inputMethodManager = ( Android.Views.InputMethods.InputMethodManager ) GetSystemService( InputMethodService )!;

      if ( CurrentFocus != null )
      {
        // Пытаемся закрыть только клавиатуру
        bool keyboardWasClosed = inputMethodManager.HideSoftInputFromWindow( CurrentFocus.WindowToken, 0 );

        if ( keyboardWasClosed )
        {
          // Если клавиатура закрылась, останавливаем дальнейшую цепочку (окно НЕ закроется)
          return;
        }
      }

      // Если клавиатуры не было, то обычное нажатие кнопки "Назад" закроет окно (стандартное поведение)
      if ( OperatingSystem.IsAndroidVersionAtLeast( 33 ) )
      { }
      else
        base.OnBackPressed();
    }


    // Вспомогательный класс для удержания связей UI-элементов одного сервера
    private class ServerInputFields
    {
      public EditText EditUrl { get; set; } = null!;
      public EditText EditPort { get; set; } = null!;
      public EditText EditPath { get; set; } = null!;
      public EditText EditEmail { get; set; } = null!;
      public EditText EditPassword { get; set; } = null!;
      public RemoteServer? OriginalServerReference { get; set; }
    }
  }

}

