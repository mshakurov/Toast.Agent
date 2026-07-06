using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Models;

public enum AgentState
{
  Starting,

  Waiting,

  Polling,

  Executing,

  Offline,

  Error,

  Stopping,

  Stopped
}
