using Toast.Core.Commands;

namespace Toast.Core.Interfaces
{
  internal interface IPollingService
  {
    Task<AgentResponse> PollAsync(
        AgentRequest request,
        CancellationToken token );

    Task ReportAsync(
        List<CommandResult> results,
        CancellationToken token );
  }
}
