using Toast.Core.Interfaces;
using Toast.Core.Logging;

namespace Toast.Core
{
  public sealed class Agent
  {
    private readonly ILogger _logger;
    private readonly IAgentSettings _settings;

    public Agent( AgentContext agentContext )
    {
      _logger = agentContext.Logger /*s?? throw new ArgumentNullException( nameof( agentContext.Logger ) )*/;
      _settings = agentContext.Settings /*?? throw new ArgumentNullException( nameof( agentContext.Settings ) )*/;
      _logger.Info( this, "Agent initialized." );
    }

    public async Task ExecuteAsync( CancellationToken token )
    {
      _logger.Info( this, "Agent started" );

      try
      {
        while ( !token.IsCancellationRequested )
        {
          await ExecuteAsyncIteration( token );

          await Task.Delay( TimeSpan.FromSeconds( 10 ), token );
        }
      }
      catch ( OperationCanceledException )
      {
      }

      _logger.Info( this, "Agent stopped" );
    }

    private async Task ExecuteAsyncIteration( CancellationToken token )
    {
      _logger.Info( this, "Tick" );

      await Task.CompletedTask;
    }
  }

}
