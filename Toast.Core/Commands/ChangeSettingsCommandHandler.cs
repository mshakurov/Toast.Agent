using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Commands.CommandData;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Commands
{
  internal class ChangeSettingsCommandHandler( IAgentServiceContext serviceContext, HostingContext context ) : ICommandHandler
  {
    private readonly IAgentServiceContext serviceContext = serviceContext;
    private readonly HostingContext context = context;

    public string CommandType => CommandTypes.ChangeSettings;

    public async Task<CommandResult> ExecuteAsync( AgentCommand command, CancellationToken cancellationToken )
    {
      var data = CommandDataConverter.GetData<ChangeSettingsData>( command );

      if ( data.data == null )
      {
        context.Logger.Error( this, data.error ?? string.Empty );
        return new CommandResult() { CommandId = command.Id, Success = false, Message = data.error };
      }

      if ( data.data.PollingInterval.HasValue && data.data.PollingInterval != context.Settings.PollingInterval )
        context.Settings.PollingInterval = Math.Max( (ushort)5, data.data.PollingInterval.Value );

      if ( data.data.RemoveServers != null && data.data.RemoveServers.Any() )
        if ( data.data.RemoveServers.Any( s => s.HostURL == "*" ) )
          context.Settings.Servers = [];
        else
          context.Settings.Servers = context.Settings.Servers.Except( data.data.RemoveServers, RemoteServer.Comparer ).ToArray();
      if ( data.data.AddServers != null && data.data.AddServers.Any() )
        context.Settings.Servers = context.Settings.Servers.Union( data.data.AddServers, RemoteServer.Comparer ).ToArray();

      try
      {
        await Task.Run( context.Settings.Update, cancellationToken );
      }
      catch ( OperationCanceledException )
      {
      }
      catch ( Exception ex )
      {
        var error = $"# Ошибка сохранения: {ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}";
        context.Logger.Error( this, error );
        return new CommandResult() { CommandId = command.Id, Success = false, Message = error };
      }

      return new CommandResult() { CommandId = command.Id, Success = true, Message = "Изменения сохранены" };
    }
  }
}
