using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Interfaces;

namespace Toast.Core.Commands
{
  internal class ShowMessageCommandHandler : ICommandHandler
  {
    public string CommandType => CommandTypes.ShowMessage;

    public async Task<CommandResult> ExecuteAsync( AgentCommand command, CancellationToken cancellationToken )
    {
      return await Task.FromResult<CommandResult>( new CommandResult() );
    }
  }
}
