using Toast.Core.Commands;

namespace Toast.Server.Data.Models
{
  public class AgentCommandFor
  {
    public long Id { get; set; }
    public Guid ClientId { get; set; } = Guid.Empty;
    public AgentCommand Command { get; set; } = AgentCommand.Empty;
  }
}
