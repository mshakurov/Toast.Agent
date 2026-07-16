namespace Toast.Core.Commands.CommandData;

public class ChangeSettingsData : CommandDataBase
{
  public ushort? PollingInterval { get; set; }
  public RemoteServer[]? AddServers { get; set; }
  public RemoteServer[]? RemoveServers { get; set; }

  public override string ToString()
  {
    string inf( RemoteServer[]? srvs ) => $"({srvs?.Length}) {string.Join( ", ", ( srvs ?? [] ).Select( s => $"[{s.GetKey()}]" ) )}";
    return $"Interval: {PollingInterval}, Srv+: {inf(AddServers)}, Srv-: {inf( RemoveServers )}";
  }
}
