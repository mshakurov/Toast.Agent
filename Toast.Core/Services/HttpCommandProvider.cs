using Toast.Core.Commands;
using Toast.Core.Interfaces;

namespace Toast.Core.Services;

public sealed class HttpCommandProvider
    : ICommandProvider
{
  public async Task<AgentResponse> GetCommandsAsync(
      CancellationToken cancellationToken )
  {
    await Task.CompletedTask;

    return new AgentResponse
    {
      PollIntervalSeconds = 10
    };
  }
}