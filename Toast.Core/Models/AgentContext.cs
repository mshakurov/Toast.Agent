using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Models
{
  public sealed class AgentContext
  {
    public required ILogger Logger { get; init; }

    public required IAgentSettings Settings { get; init; }

    public required IAgentStatusListener AgentStatusListener { get; init; }

    public required IPollingService PollingService { get; init; }
  }
}
