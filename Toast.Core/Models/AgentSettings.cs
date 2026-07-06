using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Interfaces;

namespace Toast.Core.Models
{
  public class AgentSettings : IAgentSettings
  {
    /// <summary>
    /// <inheritdoc cref="IAgentSettings.PollingInterval"/>
    /// </summary>
    public int PollingInterval { get; set; }
  }
}
