using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Models
{
  internal sealed class HostingContext
  {
    public required ILogger Logger { get; init; }

    public required IHostSettings Settings { get; init; }

    public required IHostStatusListener AgentStatusListener { get; init; }

    public required IHostShowMessage HostShowMessage { get; init; }
  }
}
