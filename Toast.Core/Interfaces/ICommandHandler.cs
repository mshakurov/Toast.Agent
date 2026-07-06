using Toast.Core.Commands;

namespace Toast.Core.Interfaces;

public interface ICommandHandler
{
  string CommandType { get; }

  Task<CommandResult> ExecuteAsync(
      AgentCommand command,
      CancellationToken cancellationToken );
}