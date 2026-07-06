namespace Toast.Core.Models;

public sealed class AgentStatus
{
  public static AgentStatus FromState( AgentState state, string? details = null )
  {
    return new AgentStatus{ State = state, Details = details };
  }

  public AgentState State { get; init; }

  public string? Details { get; init; }

  public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
