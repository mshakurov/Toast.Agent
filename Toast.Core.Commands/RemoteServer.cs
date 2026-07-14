using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Toast.Core.Commands
{
  public class RemoteServer 
  {
    public static RemoteServerComparer Comparer = new();

    public const string C_APIBasePath = "api/data/commands";

    public string? HostURL { get; set; }

    public int Port { get; set; } = 7101;

    public string BaseUrl => $"{HostURL}:{Port}";

    public string APIBasePath { get; set; } = RemoteServer.C_APIBasePath;

    public LoginModel? LoginModel { get; set; }

    public string GetKey() => $"[{HostURL}:{Port}/{APIBasePath}]@[{LoginModel?.Email}:{LoginModel?.Password}]";

    public override string ToString() => GetKey();
       

    [JsonIgnore]
    public AuthResponse? LastAuthToken { get; set; }

    public class RemoteServerComparer: IComparer<RemoteServer>, IEqualityComparer<RemoteServer>
    {
      internal RemoteServerComparer() { }

      public int Compare( RemoteServer? x, RemoteServer? y )
      {
        return string.Compare( x?.GetKey(), y?.GetKey(), StringComparison.Ordinal );
      }

      public bool Equals( RemoteServer? x, RemoteServer? y )
      {
        return Comparer.Compare( x, y ) == 0;
      }

      public int GetHashCode( [DisallowNull] RemoteServer obj )
      {
        return HashCode.Combine( obj.HostURL, obj.Port, obj.APIBasePath, obj.LoginModel?.Email, obj.LoginModel?.Password );
      }

    }
  }
}
