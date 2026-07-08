using Toast.Core.Commands;

namespace Toast.Core.Interfaces
{
  public interface IHostSettings
  {
    /// <summary>
    /// Интервал проверки наличия команд в секундах. Если значение равно < 5, то проверка выполняется раз в 5 секунд.
    /// </summary>
    ushort PollingInterval { get; set; }

    /// <summary>
    /// Список серверов, каждый в формате "http://host:port" или "https://host:port". Если список пустой, то агент не будет подключаться к серверам.
    /// </summary>
    RemoteServer[] Servers { get; set; }

    /// <summary>
    /// Последний успешный сервер, к которому агент подключался. Индекс в массиве Servers. Если значение равно 0, то агент будет подключаться к первому серверу в списке.
    /// </summary>
    ushort LastSuccessfulServerIndex { get; set; }

    /// <summary>
    /// Обновляет (сохраняет) настройки на хосте 
    /// </summary>
    /// <returns></returns>
    Task Update();
  }
}
