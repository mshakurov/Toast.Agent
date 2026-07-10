using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Commands;

public sealed class AgentResult
{
  public string AgentId { get; set; } = string.Empty;

  public List<CommandResult> Results { get; set; } = [];
}
