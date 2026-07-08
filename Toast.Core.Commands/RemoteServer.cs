using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Toast.Core.Commands
{
  public class RemoteServer
  {
    public const string C_APIBasePath = "api/data/items";

    public string? HostURL { get; set; }

    public int Port { get; set; } = 7101;

    public string APIBasePath { get; set; } = C_APIBasePath;

    public LoginModel? LoginModel { get; set; }
  
    [JsonIgnore]
    public AuthResponse? LastAuthToken { get; set; }
  }
}
