using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Commands
{
  internal class ShowMessageCommandHandler( HostingContext context ) : ICommandHandler
  {
    private readonly HostingContext context = context;

    public string CommandType => CommandTypes.ShowMessage;

    public async Task<CommandResult> ExecuteAsync( AgentCommand command, CancellationToken cancellationToken )
    {
      context.Logger.Info(this, "Command: ShowMessage");

      return await Task.FromResult<CommandResult>( new CommandResult() );
    }
  }
}
