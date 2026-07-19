using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Utilities;

namespace Toast.Core.Commands
{
  public static class NetworkDiscoverService
  {
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
            var ipAddresses = new HashSet<IPAddress>();

            string? hostName = null;
            infoLog.AppendLine( $"MachineName: {System.Environment.MachineName}" );
            infoLog.AppendLine( $"HostName: {Exec.TryGet( () => hostName = Dns.GetHostName(), ex => $"### {ex.GetFullMessage()}" )}" );
            hostName ??= System.Environment.MachineName.NullIfWhiteSpace() ?? ".";
            IPHostEntry? hostEntry = null;
            string? hostNameEntry = null;
            try
            {
              hostEntry = await Dns.GetHostEntryAsync( "127.0.0.1" );
            }
            catch ( Exception ex )
            {
              infoLog.AppendLine( $"# Can't GetHostEntry for '127.0.0.1': {ex.GetFullMessage()}" );
            }
            if ( cancelToken.IsCancellationRequested ) return infoLog.ToString();
            string[]? aliases = null;
            if ( hostEntry != null )
            {
              hostNameEntry = hostEntry.HostName;
              infoLog.AppendLine( $"HostEntry Name: {hostNameEntry}" );
              aliases = hostEntry.Aliases;
              ipAddresses.UnionWith( hostEntry.AddressList );
            }

            var names = new[] { hostName.NullIfWhiteSpace(), System.Environment.MachineName.NullIfWhiteSpace(), ".", "localhost", hostNameEntry.NullIfWhiteSpace() }.OfType<string>().Distinct( StringComparer.InvariantCultureIgnoreCase ).ToArray();

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

            infoLog.AppendLine( $"Все адреса ({ipAddresses.Count} шт.), полученные по вышеперечисленным именам:" );
            int idxIp = 0;
            foreach ( var ip in ipAddresses.OrderBy( ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 1 : ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 ? 2 : 3 ).ThenBy( ip => ip.ToString() ).ToArray() )
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
              nics = nics/*.Where( nic => nic.OperationalStatus == OperationalStatus.Up )*/.ToArray();

              infoLog.Append( $" ({nics.Length} шт.):" ).AppendLine();

              for ( int idxNic = 0; idxNic < nics.Length; idxNic++ )
              {
                var nic = nics[idxNic];

                infoLog.Append( $"{idxNic + 1}) Интерфейс: " );
                try
                {
                  infoLog.Append( $"'{nic.Name}' ({nic.OperationalStatus}), Тип: {nic.NetworkInterfaceType}. Speed: {nic.Speed}, Multicast: {nic.SupportsMulticast}, ReceiveOnly: {nic.IsReceiveOnly}, Описание: '{nic.Description}'" ).AppendLine();

                  infoLog.Append( $"Адреса UnicastAddresses" );
                  try
                  {
                    var unicastAddresses = nic.GetIPProperties().UnicastAddresses.Where( ip => !IPAddress.IsLoopback( ip.Address ) ).ToArray();
                    infoLog.Append( $" ({unicastAddresses.Length} шт.):" ).AppendLine();

                    for ( int idxIp = 0; idxIp < unicastAddresses.Length; idxIp++ )
                    {
                      var ip = unicastAddresses[idxIp];
                      infoLog.AppendLine( $"- {idxIp + 1}) {ip.Address} ({ip.IPv4Mask}) [{ip.Address.AddressFamily}{( OperatingSystem.IsWindows() ? ( ip.IsTransient ? ", (cluster)" : string.Empty ) : string.Empty )}{( OperatingSystem.IsWindows() ? $", DhcpLeaseLifetime: {ip.DhcpLeaseLifetime}" : string.Empty )}{( OperatingSystem.IsWindows() ? $", DuplicateState: {ip.DuplicateAddressDetectionState}" : string.Empty )}{( OperatingSystem.IsWindows() ? $", IsDnsEligible: {ip.IsDnsEligible}" : string.Empty )}{( OperatingSystem.IsWindows() ? $", PrefixOrigin: {ip.PrefixOrigin}" : string.Empty )}{( OperatingSystem.IsWindows() ? $", SuffixOrigin: {ip.SuffixOrigin}" : string.Empty )}]" );

                      if ( cancelToken.IsCancellationRequested ) break;
                    }
                  }
                  catch ( Exception ex )
                  {
                    infoLog.Append( " ### " ).Append( ex.GetFullMessage() ).AppendLine();
                  }

                  if ( OperatingSystem.IsWindows() )
                  {
                    infoLog.Append( $"Адреса AnycastAddresses" );
                    try
                    {
                      var anycastAddresses = nic.GetIPProperties().AnycastAddresses.Where( ip => !IPAddress.IsLoopback( ip.Address ) ).ToArray();
                      infoLog.Append( $" ({anycastAddresses.Length} шт.):" ).AppendLine();

                      for ( int idxIp = 0; idxIp < anycastAddresses.Length; idxIp++ )
                      {
                        var ip = anycastAddresses[idxIp];
                        infoLog.AppendLine( $"- {idxIp + 1}) {ip.Address} [{ip.Address.AddressFamily}{( ip.IsTransient ? ", (cluster)" : string.Empty )}, IsDnsEligible: {ip.IsDnsEligible}]" );

                        if ( cancelToken.IsCancellationRequested ) break;
                      }
                    }
                    catch ( Exception ex )
                    {
                      infoLog.Append( " ### " ).Append( ex.GetFullMessage() ).AppendLine();
                    }
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

  }
}
