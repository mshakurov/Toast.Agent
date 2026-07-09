using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.AndroidOS.Activities;
using Toast.Core.Interfaces;

namespace Toast.AndroidOS.Services;

internal class ShowMessageService : IHostShowMessage
{

  public void StartShowMessage( string message, string caption, int duration, Action<string?>? onResult )
  {
    if ( onResult != null )
      Task.Run( () =>
      {
        // 1. Регистрируем ожидание на 10 секунд и передаем делегат (OnFinishOrTimeOut)
        Guid waiterId = ActivitySignalBridge.RegisterWaiter(
            TimeSpan.FromSeconds( duration ),
            result => onResult($"TimedOut: {result.timedOut}, {result.result}")
        );

        // 2. Формируем Intent для запуска Activity1
        Intent activityIntent = new Intent( Application.Context, typeof( ShowMessageActivity ) );
        activityIntent.AddFlags( ActivityFlags.NewTask );

        // Передаем Guid в виде строки через Extra
        activityIntent.PutExtra( ActivitySignalBridge.C_EXTRA_WAITER_ID, waiterId.ToString() );
        activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraText, message );
        activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraTitle, caption );
        activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraDuration, duration );

        Application.Context.StartActivity( activityIntent );

      } );
    else
      Task.Run( () =>
      {
        // 2. Формируем Intent для запуска Activity1
        Intent activityIntent = new Intent( Application.Context, typeof( ShowMessageActivity ) );
        activityIntent.AddFlags( ActivityFlags.NewTask );

        // Передаем Guid в виде строки через Extra
        activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraText, message );
        activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraTitle, caption );
        activityIntent.PutExtra( ShowMessageActivity.C_IntentExtraDuration, duration );

        Application.Context.StartActivity( activityIntent );
      } );
  }
}
