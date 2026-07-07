using Toast.Core.Commands;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Services
{
  internal sealed class AgentService : IAgentService
  {
    private readonly HostingContext _context;

    public AgentService( HostingContext agentContext )
    {
      _context = agentContext;

      _context.Logger.Info( this, "Initialized." );
    }

    public async Task ExecuteAsync( CancellationToken token )
    {
      _context.Logger.Info( this, "Started" );

      try
      {
        while ( !token.IsCancellationRequested )
        {
          var serverConnection = CoreFactory.CreatePollingService( _context );

          AgentResponse response =
              await serverConnection.PollAsync( new AgentRequest(), token );

          _context.Logger.Info( this, $"Executing {response.Commands.Count} commands..." );
          _context.AgentStatusListener.ReportStatus( AgentState.Executing );

          var results = new List<CommandResult>();
          var dispatcher = new CommandDispatcher( CommandHandlerFactory.CreateDefault( _context ) );
          foreach ( var command in response.Commands )
          {
            var result =
                await dispatcher.ExecuteAsync(
                    command,
                    token );

            results.Add( result );
          }

          await serverConnection.ReportAsync(
              results,
              token );

          var delay =
              Math.Max( 5, _context.Settings.PollingInterval );

          _context.Logger.Info( this, "Waiting..." );
          _context.AgentStatusListener.ReportStatus( AgentState.Waiting );

          await Task.Delay(
              TimeSpan.FromSeconds( delay ),
              token );
        }
      }
      catch ( OperationCanceledException )
      {
      }
      catch ( Exception ex )
      {
        _context.Logger.Error( this, $"Error: {ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}" );
      }

      _context.Logger.Info( this, "Stopped" );
    }
  }

}
