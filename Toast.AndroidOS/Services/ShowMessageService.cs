using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.AndroidOS.Activities;
using Toast.Core.Interfaces;

namespace Toast.AndroidOS.Services;

internal class ShowMessageService( ILogger logger ) : IHostShowMessage
{

  public void StartShowMessage( string message, string caption, int duration, Action<string?>? onResult )
  {
    logger.Debug( this, $"StartShowMessage: '{message}', '{caption}', {duration}, onResult: {onResult != null}" );

    StartShowMessageNotification( message, caption, duration );

    Task.Run( () =>
    {
      logger.Debug( this, $"StartShowMessage" );

      // 2. Формируем Intent для запуска Activity1
      Intent activityIntent = new Intent( Application.Context, typeof( ShowMessageActivity ) );
      activityIntent.AddFlags( ActivityFlags.NewTask );

      if ( onResult != null )
      {
        // Регистрируем ожидание на 10 секунд и передаем делегат (OnFinishOrTimeOut)
        Guid waiterId = ActivitySignalBridge.RegisterWaiter(
            TimeSpan.FromSeconds( duration + 5 ),
            result => onResult( $"(TimedOut:{result.timedOut}) {System.Environment.NewLine}'{result.log}'" )
        );

        logger.Debug( this, $"StartShowMessage: waiterId: {waiterId}" );

        // Передаем Guid в виде строки через Extra
        activityIntent.PutExtra( ActivitySignalBridge.C_EXTRA_WAITER_ID, waiterId.ToString() );
      }
      activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraText, message );
      activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraTitle, caption );
      activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraDuration, duration );

      logger.Debug( this, $"StartShowMessage: > StartActivity" );

      Application.Context.StartActivity( activityIntent );

      logger.Debug( this, $"StartShowMessage: < StartActivity" );
    } );
  }

  public void StartShowMessageNotification( string message, string caption, int duration )
  {
    logger.Debug( this, $"StartShowMessage2: '{message}', '{caption}', {duration}" );

    // 1. Создаем обычный Intent для вашей Activity
    Intent activityIntent = new Intent( Application.Context, typeof( ShowMessageActivity ) );
    activityIntent.AddFlags( ActivityFlags.NewTask | ActivityFlags.ClearTop );
    activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraText, message );
    activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraTitle, caption );
    activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraDuration, duration );

    // 2. Оборачиваем его в PendingIntent (это дает системе право запустить его от вашего имени)
    PendingIntent? fullScreenPendingIntent = PendingIntent.GetActivity(
        Application.Context,
        0,
        activityIntent,
        PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable );

    logger.Debug( this, $"StartShowMessage2: fullScreenPendingIntent: {fullScreenPendingIntent}" );

    if ( fullScreenPendingIntent == null )
    {
      logger.Error( this, $"StartShowMessage2: Не удалось получить fullScreenPendingIntent." );
      return;
    }

    // 3. Создаем канал уведомлений (обязательно для Android 8.0+)
    string channelId = "messages_channel";
    var notificationManager = ( NotificationManager? ) Application.Context.GetSystemService( Context.NotificationService );

    logger.Debug( this, $"StartShowMessage2: notificationManager: {notificationManager}" );

    if ( notificationManager == null )
    {
      logger.Error( this, $"StartShowMessage2: Не удалось получить notificationManager." );
      return;
    }

    if ( OperatingSystem.IsAndroidVersionAtLeast( 26 ) )
    {
      var channel = new NotificationChannel( channelId, "Важные сообщения", NotificationImportance.High )
      {
        Description = "Отображение срочных окон сообщений"
      };
      channel.SetBypassDnd( true ); // Разрешить обход режима "Не беспокоить"
      notificationManager.CreateNotificationChannel( channel );

      logger.Debug( this, $"StartShowMessage2: NotificationChannel+SetBypassDnd: {notificationManager}" );
    }
    else
      logger.Warning( this, $"StartShowMessage2: Из-за версии {Build.VERSION.SdkInt} < 26 не создан NotificationChannel+SetBypassDnd." );

    // 4. Строим уведомление и прикрепляем к нему FullScreenIntent
    AndroidX.Core.App.NotificationCompat.Builder builder = new AndroidX.Core.App.NotificationCompat.Builder( Application.Context, channelId )
          .SetSmallIcon( Resource.Mipmap.appicon ) // укажите вашу иконку
          !.SetContentTitle( caption )
          !.SetContentText( message )
          !.SetPriority( AndroidX.Core.App.NotificationCompat.PriorityHigh )
          !.SetCategory( AndroidX.Core.App.NotificationCompat.CategoryCall ) // Категория звонка/будильника повышает приоритет
          !.SetFullScreenIntent( fullScreenPendingIntent, true )!; // Самая важная строка!

    logger.Debug( this, $"StartShowMessage2: builder created" );

    // 5. Показываем уведомление
    notificationManager.Notify( 1001, builder.Build() );

    logger.Debug( this, $"StartShowMessage2: Notification sent!" );
  }
}
