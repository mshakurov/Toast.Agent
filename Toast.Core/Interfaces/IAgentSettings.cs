namespace Toast.Core.Interfaces
{
  public interface IAgentSettings
  {
    /// <summary>
    /// Интервал проверки наличия команд в секундах. Если значение равно < 5, то проверка выполняется раз в 5 секунд.
    /// </summary>
    public int PollingInterval { get; set; }
  }
}
