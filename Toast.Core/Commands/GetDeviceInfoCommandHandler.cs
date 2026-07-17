using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Commands
{
  internal class GetDeviceInfoCommandHandler( IAgentServiceContext serviceContext, HostingContext context ) : ICommandHandler
  {
    private readonly IAgentServiceContext serviceContext = serviceContext;
    private readonly HostingContext context = context;

    public string CommandType => CommandTypes.GetDeviceInfo;

    public async Task<CommandResult> ExecuteAsync( AgentCommand command, CancellationToken cancellationToken ) 
      => new CommandResult { CommandId = command.Id, Success = true, Message = await context.Settings.GetDeviceInfoAsync( cancellationToken ) };
  }
}
