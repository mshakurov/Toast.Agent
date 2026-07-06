using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Services
{
  internal sealed class Agent : IAgent
  {
    private readonly AgentContext _agentContext;
    private readonly PollingService _polling;

    public Agent( AgentContext agentContext )
    {
      _agentContext = agentContext;

      _polling = new PollingService( agentContext );

      _agentContext.Logger.Info( this, "Initialized." );
    }

    public async Task ExecuteAsync( CancellationToken token )
    {
      _agentContext.Logger.Info( this, "Started" );

      try
      {
        while ( !token.IsCancellationRequested )
        {
          _agentContext.AgentStatusListener.ReportStatus( AgentState.Polling );

          await _polling.ExecuteAsync( token );

          _agentContext.AgentStatusListener.ReportStatus( AgentState.Waiting );

          await Task.Delay( Math.Max( _agentContext.Settings.PollingInterval, 5 ) * 1000, token );
        }
      }
      catch ( OperationCanceledException )
      {
      }
      catch ( Exception ex )
      {
        _agentContext.Logger.Error( this, $"Error: {ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}" );
      }

      _agentContext.Logger.Info( this, "Stopped" );
    }
  }

}
