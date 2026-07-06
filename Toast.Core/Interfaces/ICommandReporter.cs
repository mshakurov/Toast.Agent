using Toast.Core.Commands;

namespace Toast.Core.Interfaces;

public interface ICommandReporter
{
  Task ReportResultsAsync(
      IReadOnlyList<CommandResult> results,
      CancellationToken cancellationToken );
}