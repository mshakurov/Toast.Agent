using Toast.Core.Commands;
using Toast.Core.Interfaces;

namespace Toast.Core.Services;

public sealed class HttpCommandReporter
    : ICommandReporter
{
  public async Task ReportResultsAsync(
      IReadOnlyList<CommandResult> results,
      CancellationToken cancellationToken )
  {
    await Task.CompletedTask;
  }
}