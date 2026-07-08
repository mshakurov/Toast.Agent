namespace Toast.Core.Commands
{
  public class CommandDataChangeSettings
  {
    public ushort? PollingInterval { get; set; }
    public RemoteServer[]? AddServers { get; set; }
    public RemoteServer[]? RemoveServers { get; set; }
  }
}
