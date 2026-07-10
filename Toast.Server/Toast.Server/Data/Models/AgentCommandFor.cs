using Toast.Core.Commands;

namespace Toast.Server.Data.Models
{
  public class AgentCommandFor
  {
    public long Id { get; set; }
    public AgentClient? Client { get; set; }
    public AgentCommand Command { get; set; } = AgentCommand.Empty;
  }
}
