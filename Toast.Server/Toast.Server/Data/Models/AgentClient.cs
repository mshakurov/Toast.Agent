namespace Toast.Server.Data.Models
{
  public class AgentClient
  {
    public string ClientId { get; set; } = string.Empty;

    public override string ToString()
    {
      return $"{ClientId}";
    }
  }
}
