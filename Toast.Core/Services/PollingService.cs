using Toast.Core.Commands;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Services;

internal sealed class PollingService : IPollingService
{
  private readonly ILogger _logger;

  private readonly ICommandProvider _provider;
  private readonly ICommandReporter _reporter;
  private readonly CommandDispatcher _dispatcher;
  
  public PollingService( AgentContext context )
  {
    _provider = context.CommandProvider;
    _reporter = context.CommandReporter;
    _dispatcher = new CommandDispatcher( context.CommandHandlers );
    _logger = context.Logger;
  }

  public async Task ExecuteAsync(
        CancellationToken token )
  {
    while ( !token.IsCancellationRequested )
    {
      _logger.Info( this, "Polling server..." );

      AgentResponse response =
          await _provider.GetCommandsAsync( token );

      var results = new List<CommandResult>();

      foreach ( var command in response.Commands )
      {
        var result =
            await _dispatcher.ExecuteAsync(
                command,
                token );

        results.Add( result );
      }

      await _reporter.ReportResultsAsync(
          results,
          token );

      var delay =
          Math.Max( 5, response.PollIntervalSeconds );

      await Task.Delay(
          TimeSpan.FromSeconds( delay ),
          token );
    }
  }
}