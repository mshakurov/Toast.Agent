using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Models
{
  public sealed class AgentContext
  {
    public required ILogger Logger { get; init; }

    public required IAgentSettings Settings { get; init; }

    public required IAgentStatusListener AgentStatusListener { get; init; }

    public required ICommandProvider CommandProvider { get; init; }

    public required ICommandReporter CommandReporter { get; init; }

    public required IReadOnlyList<ICommandHandler> CommandHandlers { get; init; }
  }
}
