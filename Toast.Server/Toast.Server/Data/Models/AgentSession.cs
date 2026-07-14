namespace Toast.Server.Data.Models
{
  public class AgentSession
  {
    public long Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string RemoteIPAddress { get; set; } = String.Empty;
    public int RemotePort { get; set; } = 0;
    public int LocalPort { get; set; } = 0;
    public DateTime Time { get; set; } = DateTime.UtcNow;

    public string? UserIdentityName { get; set; }

  }
}
