using Toast.Core.Interfaces;

namespace Toast.Core.Services;

internal sealed class PollingService : IPollingService
{
  private readonly ILogger _logger;

  public PollingService( ILogger logger )
  {
    _logger = logger;
  }

  public Task ExecuteAsync( CancellationToken token )
  {
    _logger.Info( this, "Polling..." );

    return Task.CompletedTask;
  }
}