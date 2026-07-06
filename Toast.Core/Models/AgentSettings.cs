using Toast.Core.Interfaces;

namespace Toast.Core.Models;

public class AgentSettings : IAgentSettings
{
  /// <summary>
  /// <inheritdoc cref="IAgentSettings.PollingInterval"/>
  /// </summary>
  public int PollingInterval { get; set; } = 10;
}
