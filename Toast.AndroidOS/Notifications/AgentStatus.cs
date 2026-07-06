using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Models;

namespace Toast.AndroidOS.Notifications;

public sealed class AgentStatus
{
  public AgentState State { get; init; }

  public string? Details { get; init; }

  public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
