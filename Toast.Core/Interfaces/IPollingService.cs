using Toast.Core.Commands;

namespace Toast.Core.Interfaces
{
  internal interface IPollingService
  {
    Task<AgentResponse> PollAsync(
        AgentRequest request,
        CancellationToken token );

    Task ReportAsync(
        IReadOnlyList<CommandResult> results,
        CancellationToken token );
  }
}
