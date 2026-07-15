namespace Toast.Core.Commands.CommandData;

public class ChangeSettingsData : CommandDataBase
{
  public ushort? PollingInterval { get; set; }
  public RemoteServer[]? AddServers { get; set; }
  public RemoteServer[]? RemoveServers { get; set; }

  public override string ToString()
  {
    return $"Interval: {PollingInterval}, Srv+: {AddServers?.Length}, Srv-: {RemoveServers?.Length}";
  }
}
