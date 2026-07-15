namespace Toast.Core.Commands.CommandData;

public class ChangeSettingsData : CommandDataBase
{
  public ushort? PollingInterval { get; set; }
  public RemoteServer[]? AddServers { get; set; }
  public RemoteServer[]? RemoveServers { get; set; }
}
