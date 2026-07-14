using Toast.Core.Commands;

namespace Toast.Server.Data.Models
{
  public class AgentCommandFor
  {
    public long Id { get; set; }
    public string ClientId { get; set; } = null!;
    public AgentClient Client { get; set; } = null!;
    public AgentCommand Command { get; set; } = AgentCommand.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime? Sent { get; set; }

    public override string ToString()
    {
      return $"{Id}, ClientId: {ClientId}: '{Command}', Cr:{Created:HH:mm:ss-yy.MM.dd}, Sent:{Sent:HH:mm:ss-yy.MM.dd}";
    }
  }
}
