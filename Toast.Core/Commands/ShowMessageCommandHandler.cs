using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Commands.CommandData;
using Toast.Core.Utilities;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Commands
{
  internal class ShowMessageCommandHandler( IAgentServiceContext serviceContext, HostingContext context ) : ICommandHandler
  {
    private readonly IAgentServiceContext serviceContext = serviceContext;
    private readonly HostingContext context = context;

    public string CommandType => CommandTypes.ShowMessage;

    public async Task<CommandResult> ExecuteAsync( AgentCommand command, CancellationToken cancellationToken )
    {
      context.Logger.Info( this, "Command: ShowMessage" );

      var data = CommandDataConverter.GetData<ShowMessageData>( command );

      if ( data.data == null )
      {
        context.Logger.Error( this, data.error ?? string.Empty );
        return new CommandResult() { CommandId = command.Id, Success = false, Message = data.error };
      }

      try
      {
        if ( data.data.WaitIfShow )
        {
          _ = StartWaitShowMessage( command.Id, data.data, cancellationToken );

          return await Task.FromResult( new CommandResult() { CommandId = command.Id, Success = true, Message = $"Сообщение отправлено на устройство, результат в следующем сообщении" } );
        }
        else
        {
          context.HostShowMessage.StartShowMessage( data.data.Message, data.data.Title, data.data.Duration, null );

          return await Task.FromResult( new CommandResult() { CommandId = command.Id, Success = true, Message = $"Отправлено на устройство" } );
        }
      }
      catch ( Exception ex )
      {
        var error = $"Ошибка отображения сообщения: {ex.GetFullMessage()}";
        context.Logger.Error( this, error );
        return new CommandResult() { CommandId = command.Id, Success = false, Message = error };
      }
    }

    private Task StartWaitShowMessage( Guid commandId, ShowMessageData data, CancellationToken cancellationToken )
      => Task.Run( ()
        => context.HostShowMessage.StartShowMessage( data.Message, data.Title, data.Duration, message => OnShowResult( commandId, message ) ), cancellationToken );

    private void OnShowResult( Guid commandId, string? message )
    {
      context.Logger.Debug(this, $"OnShowResult: {commandId}, '{message}'" );

      serviceContext.AddCommandResult( new CommandResult()
      {
        CommandId = commandId,
        Success = true,
        Message = $"Показано на устройстве{( message == null ? string.Empty : $", '{message}'" )}"
      } );
    }
  }
}
