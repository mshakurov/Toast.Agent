using Toast.Core.Interfaces;
using Toast.Core.Logging;
using Toast.Core.Services;

namespace Toast.Core
{
  public sealed class Agent
  {
    private readonly ILogger _logger;
    private readonly IAgentSettings _settings;
    private readonly IPollingService _pollingService;

    public Agent( AgentContext agentContext )
    {
      _logger = agentContext.Logger /*s?? throw new ArgumentNullException( nameof( agentContext.Logger ) )*/;
      _settings = agentContext.Settings /*?? throw new ArgumentNullException( nameof( agentContext.Settings ) )*/;
      _pollingService = new PollingService( _logger );
      
      _logger.Info( this, "Initialized." );
    }

    public async Task ExecuteAsync( CancellationToken token )
    {
      _logger.Info( this, "Started" );

      try
      {
        while ( !token.IsCancellationRequested )
        {
          await _pollingService.ExecuteAsync( token );

          await Task.Delay( _settings.PollingInterval * 1000, token );
        }
      }
      catch ( OperationCanceledException )
      {
      }
      catch ( Exception ex )
      {
        _logger.Error( this, $"Error: {ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}" );
      }

      _logger.Info( this, "Stopped" );
    }
  }

}
