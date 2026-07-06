using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Interfaces;

namespace Toast.Core
{
  public sealed class AgentContext
  {
    public required ILogger Logger { get; init; }

    public required IAgentSettings Settings { get; init; }
  }
}
