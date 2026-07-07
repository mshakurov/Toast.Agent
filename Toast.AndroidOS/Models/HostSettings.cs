
using Toast.Core.Interfaces;

namespace Toast.AndroidOS.Models;

public class HostSettings : IHostSettings
{
  /// <summary>
  /// <inheritdoc cref="IHostSettings.PollingInterval"/>
  /// </summary>
  public int PollingInterval { get; set; } = 10;

  /// <summary>
  /// <inheritdoc cref="IHostSettings.Update"/>
  /// </summary>
  public Task Update()
  {
    throw new NotImplementedException();
  }
}
