using System.Text.Json;

using Toast.Core.Commands;

namespace Toast.Server.Data.Models
{
  public class RemoteServerDB
  {
    public int Id { get; set; }
    public string Json { get; set; } = string.Empty;

    public RemoteServer? GetRemoteServer()
    {
      return JsonSerializer.Deserialize<RemoteServer>(Json);
    }

    public static RemoteServerDB CreateFrom( RemoteServer remoteServer )
    {
      return new RemoteServerDB { Json = JsonSerializer.Serialize( remoteServer ) };
    }
  }
}
