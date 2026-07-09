using Toast.Core.Commands;
using Toast.Core.Interfaces;

namespace Toast.Core.Services;

internal sealed class CommandDispatcher
{
  private readonly IReadOnlyDictionary<string, ICommandHandler> _handlers;
  private readonly CancellationToken cancellationToken;

  public CommandDispatcher(
      IEnumerable<ICommandHandler> handlers, CancellationToken cancellationToken )
  {
    _handlers = handlers.ToDictionary(
        x => x.CommandType,
        StringComparer.OrdinalIgnoreCase );
    this.cancellationToken = cancellationToken;
  }

  public async Task<CommandResult> ExecuteAsync(
      AgentCommand command )
  {
    if ( !_handlers.TryGetValue( command.Type, out var handler ) )
    {
      return new CommandResult
      {
        CommandId = command.Id,
        Success = false,
        Message = $"Unknown command: {command.Type}"
      };
    }

    try
    {
      return await handler.ExecuteAsync(
        command,
        cancellationToken );
    }
    catch ( Exception ex )
    {
      return new CommandResult
      {
        CommandId = command.Id,
        Success = false,
        Message = $"Error executing: {ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}"
      };
    }
  }
}