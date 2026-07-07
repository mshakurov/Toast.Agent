namespace Toast.Core.Interfaces
{
  public interface IHostSettings
  {
    /// <summary>
    /// Интервал проверки наличия команд в секундах. Если значение равно < 5, то проверка выполняется раз в 5 секунд.
    /// </summary>
    int PollingInterval { get; set; }

    /// <summary>
    /// Обновляет (сохраняет) настройки на хосте 
    /// </summary>
    /// <returns></returns>
    Task Update();
  }
}
