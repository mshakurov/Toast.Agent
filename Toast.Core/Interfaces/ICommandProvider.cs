using Toast.Core.Commands;

namespace Toast.Core.Interfaces;

public interface ICommandProvider
{
  Task<AgentResponse> GetCommandsAsync(
      CancellationToken cancellationToken );
}