namespace Toast.Server.Data.Models
{
  public class AgentClient
  {
    public string ClientId { get; set; } = string.Empty;

    public DateTime LastGet { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastSet { get; set; }

    public override string ToString()
    {
      return $"{ClientId}";
    }
  }
}
