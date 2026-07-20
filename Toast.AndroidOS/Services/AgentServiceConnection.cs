using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.AndroidOS.Activities;

namespace Toast.AndroidOS.Services
{
  internal class AgentServiceConnection : Java.Lang.Object, IServiceConnection
  {
    private readonly MainActivity _activity;

    public AgentServiceConnection( MainActivity activity )
    {
      _activity = activity;
    }

    // Вызывается автоматически, когда связь установлена
    public void OnServiceConnected( ComponentName? name, IBinder? service )
    {
      // Приводим прилетевший IBinder к нашему кастомному классу Биндера
      if ( service is AgentService.LocalBinder binder )
      {
        // МАГИЯ: Получаем прямую ссылку на работающий сервис!
        _activity.AgentService = binder.Service;

        // Сразу можем вызвать метод обновления UI в Activity
        _activity.OnServiceSuccessfullyConnected();
      }
    }

    // Вызывается, если сервис аварийно упал
    public void OnServiceDisconnected( ComponentName? name )
    {
      _activity.AgentService = null;

      // Сразу можем вызвать метод обновления UI в Activity
      _activity.OnServiceDisconnected();
    }
  }
}
