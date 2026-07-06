using Toast.Core.Interfaces;
using Toast.Core.Logging;
using Toast.Core.Models;
using Toast.Core.Services;

namespace Toast.Core
{
  internal sealed class Agent : IAgent
  {
    private readonly ILogger _logger;
    private readonly IAgentSettings _settings;
    private readonly IAgentStatusListener _agentStatusListener;
    private readonly IPollingService _pollingService;

    public Agent( AgentContext agentContext )
    {
      _logger = agentContext.Logger ?? throw new ArgumentNullException( nameof( agentContext.Logger ) );
      _settings = agentContext.Settings ?? throw new ArgumentNullException( nameof( agentContext.Settings ) );
      _agentStatusListener = agentContext.AgentStatusListener ?? throw new ArgumentNullException( nameof( agentContext.AgentStatusListener ) );

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
          _agentStatusListener.ReportStatus( AgentState.Polling );

          await _pollingService.ExecuteAsync( token );

          _agentStatusListener.ReportStatus( AgentState.Waiting );

          await Task.Delay( Math.Max( _settings.PollingInterval, 5 ) * 1000, token );
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
