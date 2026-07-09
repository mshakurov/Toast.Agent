namespace Toast.Core.Commands.CommandData;

public class ChangeSettingsData
{
  public ushort? PollingInterval { get; set; }
  public RemoteServer[]? AddServers { get; set; }
  public RemoteServer[]? RemoveServers { get; set; }
}
