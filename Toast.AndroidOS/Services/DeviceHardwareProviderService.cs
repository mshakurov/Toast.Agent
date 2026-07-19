using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Android.OS;

using Toast.Core.Commands;
using Toast.Core.Utilities;

namespace Toast.AndroidOS.Services;

public static class DeviceHardwareProviderService
{
  public static string GetDeviceHumanReadableName()
  {
    // 1. Производитель (например: Samsung, Xiaomi, Google, LGE)
    try
    {
      string? manufacturer = Build.Manufacturer;

      // 2. Бренд / Торговая марка (например: samsung, POCO, google)
      string? brand = Build.Brand;

      // 3. Рыночное название модели или код (например: SM-G998B, POCO X3 Pro, Pixel 7)
      string? model = Build.Model;

      return $"{brand} {model} ({manufacturer}, {Build.Device}, {Build.Hardware}, {Build.Host}, {Build.Type})";
    }
    catch ( Exception ex )
    {
      return $"# Can't read 'Build.Manufacturer': {ex.GetFullMessage()}";
    }
  }

  public static async Task<string> GetDeviceHumanReadableFullInfo( CancellationToken token = default )
  {
    try
    {
      var log = new List<string>();

      log.Add( "*** Build ***" );
      log.Add( GetValues( typeof( Build ).GetProperties( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static ).Where( p => p.CanRead && p.GetIndexParameters().Length == 0 ).OrderBy( p => 
        p.Name.Equals( "Fingerprint", StringComparison.InvariantCultureIgnoreCase ) ? 1
        : p.Name.Equals( "Brand", StringComparison.InvariantCultureIgnoreCase ) ? 2
        : p.Name.Equals( "Model", StringComparison.InvariantCultureIgnoreCase ) ? 3
        : p.Name.Equals( "Device", StringComparison.InvariantCultureIgnoreCase ) ? 4
        : int.MaxValue 
        ).ToArray() ) );

      log.Add( "*** Build.VERSION ***" );
      log.Add( GetValues( typeof( Build.VERSION ).GetProperties( System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static ).Where( p => p.CanRead && p.GetIndexParameters().Length == 0 ).ToArray() ) );

      log.Add( "*** Host/Network ***" );
      log.Add( await NetworkDiscoverService.GetHostNetworkInfoAsync( token ) );

      return string.Join( System.Environment.NewLine, log );
    }
    catch ( Exception ex )
    {
      return $"# Can't read Device Info': {ex.GetFullMessage()}";
    }
  }

  public static string GetValues( PropertyInfo[] properties )
  {
    try
    {
      var log = new List<string>( properties.Length );
      foreach ( var prop in properties )
      {
        string? strValue = null;
        try
        {
          strValue = prop.GetValue( null )?.ToString();
        }
        catch ( Exception ex )
        {
          strValue = $"# {ex.Message}";
        }
        log.Add( $"- {prop.Name}: [{strValue}];" );
      }
      return string.Join( System.Environment.NewLine, log );
    }
    catch ( Exception ex )
    {
      return $"# Can't read Device Info': {ex.GetFullMessage()}";
    }
  }

}
