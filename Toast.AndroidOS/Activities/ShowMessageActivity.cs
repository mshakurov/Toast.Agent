using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.OS;
using Android.Views;

using Toast.AndroidOS.Bootstrap;
using Toast.Core.Utilities;
using Toast.Core.Interfaces;

namespace Toast.AndroidOS.Activities;

[Activity( Label = "Просмотр сообщения", Theme = "@android:style/Theme.DeviceDefault.Dialog" )]
public class ShowMessageActivity : Activity
{
  private const double C_PortraitSize = 0.8;
  private const double C_LandscapeSize = 0.5;

  public const string C_IntentExtraText = "text";
  public const string C_IntentExtraDuration = "duration";
  public const string C_IntentExtraTitle = "title";

  ILogger? logger;

  int durationSeconds = 5;
  string initialButtonText = "OK";
  string title = "Сообщение";
  Guid? _waiterId;

  TextView? textView;
  Button? okButton;

  protected override void OnCreate( Bundle? savedInstanceState )
  {
    base.OnCreate( savedInstanceState );

    logger = CompositionRoot.GetSingletonLogger();

    logger.Debug( this, $"OnCreate ({savedInstanceState?.KeySet()?.Count} keys)" );

    savedInstanceState?.KeySet()?.ToList().ForEach( key =>
    {
      var value = savedInstanceState.GetString( key );
      logger.Debug( this, $"SavedInstanceState: {key} = {value}" );
    } );

    SetContentView( Resource.Layout.activity_show_message );

    // 1. Читаем текст из Intent
    string rawText = Intent?.GetStringExtra( ShowMessageActivity.C_IntentExtraText ) ??
#if DEBUG
    string.Join( System.Environment.NewLine, Enumerable.Range( 1, 10 ).Select( i => $"Строка {i} с длинным с длинным с длинным с длинным текстом, аж очень длинным до невозможности." ) )
#else
    "Сообщение"
#endif
      ;
    durationSeconds = Intent?.GetIntExtra( ShowMessageActivity.C_IntentExtraDuration, durationSeconds ) ?? durationSeconds;
    title = Intent?.GetStringExtra( ShowMessageActivity.C_IntentExtraTitle ) ?? title;

    _waiterId = Guid.TryParse( Intent?.GetStringExtra( ActivitySignalBridge.C_EXTRA_WAITER_ID ), out var id ) ? id : null;

    logger.Debug( this, $"OnCreate: durationSeconds={durationSeconds}, title:'{title}', rawText=({rawText.Length})'{rawText.Substring( 0, Math.Min( rawText.Length, 30 ) )}'. waiter:{_waiterId}" );

    // 2. Заменяем комбинацию "\n" на настоящий перевод строки
    string displayText = rawText.Replace( "\\n", System.Environment.NewLine );

    // 3. Находим TextView и вставляем текст
    textView = FindViewById<TextView>( Resource.Id.textShowMessageMessage );
    if ( textView != null )
      textView.Text = displayText;

    this.Title = title;

    CancellationTokenSource _cts = new();

    // 4. Кнопка OK – закрывает окно
    okButton = FindViewById<Button>( Resource.Id.buttonShowMessageOK );
    if ( okButton != null )
    {
      initialButtonText = okButton.Text ?? initialButtonText;
      okButton.Click += ( sender, e ) =>
      {
        _cts.Cancel();
        SignalWaiter( "OK Clicked" );
        if ( !IsFinishing )
          FinishAndRemoveTask();
      };

      okButton.RequestFocus();
      okButton.Post( () => okButton.RequestFocus() );
      okButton.PostDelayed( () => okButton.RequestFocus(), 100 );
    }

    // 5. Делаем окно на 50% экрана и по центру
    SetupWindowSize();

    if ( okButton != null )
      StartTextChange( _cts.Token );
    else
      // Запуск авто-закрытия по таймауту
      _ = AutoCloseAfterDelay( durationSeconds, _cts.Token );
  }

  protected override void OnStart()
  {
    base.OnStart();

    // Оповещаем, если был ожидатель
    SignalWaiter( "After OnStart" );

    logger?.Debug( this, "OnStart" );
  }

  public override void OnAttachedToWindow()
  {
    base.OnAttachedToWindow();

    // Оповещаем, если был ожидатель
    SignalWaiter( "After OnAttachedToWindow" );

    logger?.Debug( this, "OnAttachedToWindow" );
  }

  public override void OnWindowFocusChanged( bool hasFocus )
  {
    base.OnWindowFocusChanged( hasFocus );

    // Оповещаем, если был ожидатель
    SignalWaiter( "After OnWindowFocusChanged" );

    logger?.Debug( this, $"OnWindowFocusChanged({hasFocus})" );
  }

  private void SetupWindowSize()
  {
    if ( Window == null || Resources == null || Resources.DisplayMetrics == null )
      return;

    var displayMetrics = Resources.DisplayMetrics;
    int width = ( int ) ( displayMetrics.WidthPixels * ( displayMetrics.WidthPixels > displayMetrics.HeightPixels ? C_LandscapeSize : C_PortraitSize ) );
    int height = ( int ) ( displayMetrics.HeightPixels * ( displayMetrics.HeightPixels > displayMetrics.WidthPixels ? C_LandscapeSize : C_PortraitSize ) );

    Window.SetLayout( width, height );
    Window.SetGravity( GravityFlags.Center );
    // Убираем рамку диалога, если тема не полностью подходит
    //Window.SetBackgroundDrawableResource( Android.Resource.Color.Transparent );

    // Оповещаем, если был ожидатель
    SignalWaiter( "After SetupWindowSize" );
  }

  private Task StartTextChange( CancellationToken token )
  {
    /* проверка 'okButton == null' - чтобы компилятор не варнинговал */
    if ( okButton == null ) return Task.CompletedTask;

    return Task.Factory.StartNew( () =>
    {
      SignalWaiter( $"TextChange Started" );

      // счетчик сразу не стартуем, чтобы в первый раз нарисовать durationSeconds и после этого начать отсчет
      System.Diagnostics.Stopwatch stopwatch = new();
      // чтобы в первый раз написать durationSeconds секунд
      int lastRemainingSecondsSeconds = durationSeconds + 1;
      int GetRemainingSeconds() => ( int ) Math.Ceiling( durationSeconds - stopwatch.Elapsed.TotalSeconds );
      bool finished = false;
      while ( !token.IsCancellationRequested && !IsFinishing )
      {
        int remainingSeconds = GetRemainingSeconds();
        if ( remainingSeconds <= 0 )
        {
          finished = true;
          stopwatch.Stop();

          SignalWaiter( $"TextChange Finished ({remainingSeconds})", true );

          if ( !IsFinishing )
            FinishAndRemoveTask();

          break;
        }
        if ( lastRemainingSecondsSeconds != remainingSeconds )
        {
          lastRemainingSecondsSeconds = remainingSeconds;

          WaitOnUI( token =>
          {
            try { okButton.Text = $"{initialButtonText}{$" ({remainingSeconds} сек)"}"; }
            catch { }

            if ( !stopwatch.IsRunning )
              stopwatch.Start();
          }, token );
        }
        try
        {
          Task.Delay( TimeSpan.FromMilliseconds( 100 ), token ).Wait( token );
        }
        catch
        {
          // Игнорируем ошибки
        }
      }

      if( !finished )
        SignalWaiter( $"TextChange Aborted ({GetRemainingSeconds()})", true );
    }, token );
  }

  void WaitOnUI( Action<CancellationToken> act, CancellationToken token )
  {
    var tcs = new TaskCompletionSource();

    RunOnUiThread( () =>
    {
      try
      {
        act( token );

        tcs.SetResult();
      }
      catch ( Exception ex )
      {
        tcs.SetException( ex );
      }
    } );

    try { tcs.Task.Wait( token ); } 
    catch (Exception ex)
    {
      logger?.Debug( this, $"# WaitOnUI Error: {ex.GetFullMessage()}" );
    }
  }

  private async Task AutoCloseAfterDelay( int durationSeconds, CancellationToken token )
  {
    try
    {
      await Task.Delay( TimeSpan.FromSeconds( durationSeconds ), token );
    }
    catch
    {
      // Игнорируем ошибки
    }

    SignalWaiter( "AutoCloseAfterDelay", true );

    if ( !IsFinishing )
      FinishAndRemoveTask();
  }

  void SignalWaiter( string message, bool finish = false )
  {
    // Оповещаем, если был ожидатель
    if ( _waiterId != null )
    {
      //logger?.Debug( this, $"OnStart: > ActivitySignalBridge.Signal('After SetupWindowSize')" );

      if ( finish )
        ActivitySignalBridge.Finish( _waiterId.Value, message );
      else
        ActivitySignalBridge.Signal( _waiterId.Value, message );

      //logger?.Debug( this, $"OnStart: < ActivitySignalBridge.Signal('After SetupWindowSize')" );
    }
  }
}
