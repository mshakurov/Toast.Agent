using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Android.OS;

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
      log.Add( await GetHostNetworkInfoAsync( token ) );

      return string.Join( System.Environment.NewLine, log );
    }
    catch ( Exception ex )
    {
      return $"# Can't read Device Info': {ex.GetFullMessage()}";
    }
  }

  public static async Task<string> GetHostNetworkInfoAsync( CancellationToken cancelToken = default )
  {
    return await Task.Run( async () =>
    {
      var infoLog = new StringBuilder();
      infoLog.AppendLine()
        .AppendLine( $"@ >>> --- Информация об адресах, именах и интерфейсах:" );

      var task1 = Exec.TryGet( () => Task.Run( async () =>
      {
        var infoLog = new StringBuilder();

        infoLog.AppendLine( $"@ > --- Информация об именах и адресах" );

        try
        {
          string? hostName = null;
          infoLog.AppendLine( $"MachineName: {System.Environment.MachineName}" );
          infoLog.AppendLine( $"HostName: {Exec.TryGet( () => hostName = Dns.GetHostName(), ex => $"### {ex.GetFullMessage()}" )}" );
          hostName ??= System.Environment.MachineName.NullIfWhiteSpace() ?? ".";

          var names = new[] { hostName.NullIfWhiteSpace(), System.Environment.MachineName.NullIfWhiteSpace(), ".", "localhost" }.OfType<string>().Distinct( StringComparer.InvariantCultureIgnoreCase ).ToArray();
          var ipAddresses = new HashSet<IPAddress>();

          foreach ( var name in names )
          {
            infoLog.Append( $"Адреса по имени '{name}': " );
            try
            {
              var addresses = await Dns.GetHostAddressesAsync( name, cancelToken );
              infoLog.Append( $"({addresses.Length} шт.)" ).AppendLine();
              ipAddresses.UnionWith( addresses );
            }
            catch ( Exception ex )
            {
              infoLog.Append( "### " ).Append( ex.GetFullMessage() ).AppendLine();
            }
            if ( cancelToken.IsCancellationRequested ) break;
          }

          var addressList = ipAddresses.Where( ip => !IPAddress.IsLoopback( ip ) ).ToArray();
          infoLog.AppendLine( $"Все адреса ({addressList.Length} шт.), полученные по вышеперечисленным именам:" );
          int idxIp = 0;
          foreach ( var ip in ipAddresses.Where( ip => !IPAddress.IsLoopback( ip ) ).OrderBy( ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 1 : ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? 2 : 3 ).ThenBy( ip => ip.ToString() ).ToArray() )
            infoLog.AppendLine( $"- {++idxIp}) {ip} [{ip.AddressFamily}]" );
        }
        catch ( Exception exCommon )
        {
          infoLog.Append( "### " ).Append( exCommon.GetFullMessage() ).AppendLine();
        }
        infoLog.AppendLine( $"@ < --- Информация об именах и адресах" );

        return infoLog.ToString();
      }, cancelToken ), ex =>
      {
        infoLog.AppendLine( $"### Ошибка старта задачи 'Информация об именах и адресах'" );
        return null;
      } );

      var task2 = Exec.TryGet( () => Task.Run( () =>
      {
        var infoLog = new StringBuilder();

        infoLog.AppendLine( $"@ > --- Информация об интерфейсах" );
        var nics = Exec.TryGet( () => NetworkInterface.GetAllNetworkInterfaces(), ex =>
        {
          infoLog.Append( " ### " ).Append( ex.GetFullMessage() ).AppendLine();
          return null;
        } );

        if ( cancelToken.IsCancellationRequested ) return infoLog.ToString();

        if ( nics != null )
        {
          try
          {
            nics = nics.Where( nic => nic.OperationalStatus == OperationalStatus.Up ).ToArray();

            infoLog.Append( $" ({nics.Length} шт.):" ).AppendLine();

            for ( int idxNic = 0; idxNic < nics.Length; idxNic++ )
            {
              var nic = nics[idxNic];

              infoLog.Append( $"{idxNic + 1}) Интерфейс: " );
              try
              {
                infoLog.Append( $"'{nic.Name}', Тип: {nic.NetworkInterfaceType}. Описание: '{nic.Description}'" ).AppendLine();

                infoLog.Append( $"Адреса" );
                try
                {
                  var unicastAddresses = nic.GetIPProperties().UnicastAddresses.Where( ip => !IPAddress.IsLoopback( ip.Address ) ).ToArray();
                  infoLog.Append( $" ({unicastAddresses.Length} шт.):" ).AppendLine();

                  for ( int idxIp = 0; idxIp < unicastAddresses.Length; idxIp++ )
                  {
                    var ip = unicastAddresses[idxIp];
                    infoLog.AppendLine( $"- {idxIp + 1}) {ip.Address} [{ip.Address.AddressFamily}{( OperatingSystem.IsWindows() ? ( ip.IsTransient ? ", (cluster)" : string.Empty ) : string.Empty )}]" );

                    if ( cancelToken.IsCancellationRequested ) break;
                  }
                }
                catch ( Exception ex )
                {
                  infoLog.Append( " ### " ).Append( ex.GetFullMessage() ).AppendLine();
                }
              }
              catch ( Exception exNic )
              {
                infoLog.Append( " ### " ).Append( exNic.GetFullMessage() ).AppendLine();
              }
              infoLog.AppendLine( "---" );
            }
          }
          catch ( Exception exNics )
          {
            infoLog.Append( "### " ).Append( exNics.GetFullMessage() ).AppendLine();
          }
        }
        infoLog.AppendLine( $"@ < --- Информация об интерфейсах" );

        return infoLog.ToString();
      }, cancelToken ), ex =>
      {
        infoLog.AppendLine( $"### Ошибка старта задачи 'Информация об интерфейсах'" );
        return null;
      } );

      if ( task1 != null || task2 != null )
      {
        try
        {
          await Task.WhenAny( Task.WhenAll( new[] { task1, task2 }.OfType<Task<string>>().ToArray() ), Task.Run( () => cancelToken.WaitHandle.WaitOne() ) );
        }
        catch ( System.OperationCanceledException ) { }
        catch ( Exception ex )
        {
          infoLog.AppendLine( $"### Ошибка ожидания задач: {ex.GetFullMessage()}" );
        }

        if ( task1 != null )
          infoLog.AppendLine( task1.Result.NullIfWhiteSpace() ?? string.Empty );
        if ( task2 != null )
          infoLog.AppendLine( task2.Result.NullIfWhiteSpace() ?? string.Empty );
      }

      infoLog.AppendLine( $"@ <<< --- Информация об адресах, именах и интерфейсах" );

      return infoLog.ToString();
    }, cancelToken );
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
