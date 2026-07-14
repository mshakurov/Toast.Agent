namespace Toast.Server.Data.Models
{
  public class AgentSessionInfo
  {
    public string RemoteIPAddress { get; set; } = String.Empty;
    public int RemotePort { get; set; } = 0;
    public int LocalPort { get; set; } = 0;
    
  }
}
