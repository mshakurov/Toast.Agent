using Toast.Core.Commands;

namespace Toast.Server.Data.Models
{
  public class AgentResultDB
  {
    public AgentResultDB() { }

    public long Id { get; set; }

    public string AgentId { get; set; } = string.Empty;

    public List<CommandResult> Results { get; set; } = [];

    public static AgentResultDB From( AgentResult agentResult ) 
      => new AgentResultDB { AgentId = agentResult.AgentId, Results = agentResult.Results };
  }
}
