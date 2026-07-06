namespace Toast.Core.Commands;

public sealed class AgentResponse
{
  public int PollIntervalSeconds { get; set; }

  public List<AgentCommand> Commands { get; set; } = [];
}