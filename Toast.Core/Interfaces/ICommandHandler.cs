using Toast.Core.Commands;

namespace Toast.Core.Interfaces;

internal interface ICommandHandler
{
  string CommandType { get; }

  Task<CommandResult> ExecuteAsync(
      AgentCommand command,
      CancellationToken cancellationToken );
}