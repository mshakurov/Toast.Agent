using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Commands
{
  internal class ChangeSettingsCommandHandler( HostingContext context ) : ICommandHandler
  {
    private readonly HostingContext context = context;

    public string CommandType => CommandTypes.ChangeSettings;

    public async Task<CommandResult> ExecuteAsync( AgentCommand command, CancellationToken cancellationToken )
    {
      CommandDataChangeSettings? data = null;

      try
      {
        data = command.Parameters.Deserialize<CommandDataChangeSettings>();
      }
      catch ( Exception ex )
      {
        return new CommandResult() { CommandId = command.Id, Success = false, Message = $"Ошибка десериализации Parameters: {ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}" };
      }

      if ( data == null ) return new CommandResult() { CommandId = command.Id, Success = false, Message = "Parameters десериализовался в null" };

      var changed = false;

      if ( data.PollingInterval.HasValue && data.PollingInterval >= 5 &&  data.PollingInterval != context.Settings.PollingInterval )
        context.Settings.PollingInterval = data.PollingInterval.Value;

      if ( changed )
        try
        {
          await context.Settings.Update();
        }
        catch ( Exception ex )
        {
          return new CommandResult() { CommandId = command.Id, Success = false, Message = $"Ошибка сохранения: {ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}" };
        }
      else
        return new CommandResult() { CommandId = command.Id, Success = false, Message = "Не обнаружены изменения" };

      return new CommandResult() { CommandId = command.Id, Success = true, Message = "Изменения сохранены" };
    }
  }
}
