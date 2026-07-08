
using System.Text.Json;

using Toast.AndroidOS.Bootstrap;
using Toast.AndroidOS.Services;
using Toast.Core.Commands;
using Toast.Core.Interfaces;

namespace Toast.AndroidOS.Models;

public class HostSettings : IHostSettings, ICloneable
{
  /// <summary>
  /// <inheritdoc cref="IHostSettings.PollingInterval"/>
  /// </summary>
  public ushort PollingInterval { get; set; } = 10;

  public RemoteServer[] Servers { get; set; } =
    [
      new RemoteServer { HostURL = "https://77.51.228.159", Port = 7101, APIBasePath = "api/data/items", LoginModel = new LoginModel("mshakurov@yandex.ru", "VorgeN2010$") },
      new RemoteServer { HostURL = "https://192.168.1.252", Port = 7101, APIBasePath = "api/data/items", LoginModel = new LoginModel("mshakurov@yandex.ru", "VorgeN2010$") }
    ];

  public ushort LastSuccessfulServerIndex { get; set; } = 0;

  public object Clone() => CloneTyped();

  public HostSettings CloneTyped() => JsonSerializer.Deserialize<HostSettings>(JsonSerializer.Serialize( this ) ) ?? new HostSettings();

  /// <summary>
  /// <inheritdoc cref="IHostSettings.Update"/>
  /// </summary>
  public Task Update()
  {
    return Task.Run( () => CompositionRoot.GetSingletonSettingsService().SaveSettings( this.CloneTyped() ) );
  }
}
