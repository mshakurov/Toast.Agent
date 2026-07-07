using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Commands
{
  public class CommandDataChangeSettings
  {
    public int? PollingInterval { get; set; }
    public string[]? AddServers { get; set; }
    public string[]? RemoveServers { get; set; }
  }
}
