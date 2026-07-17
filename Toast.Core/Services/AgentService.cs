using System.Collections.Concurrent;

using Toast.Core.Commands;
using Toast.Core.Utilities;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Services
{
  internal sealed class AgentService : IAgentService
  {
    private HostingContext _context;
    private readonly AgentServiceContext _serviceContext;
    private readonly ConcurrentQueue<CommandResult> _postponedResults = new();

    public AgentService( HostingContext agentContext )
    {
      _context = agentContext;
      _serviceContext = new( this );
      _context.Logger.Info( this, "Initialized." );
    }

    public async Task ExecuteAsync( CancellationToken token )
    {
      _context.Logger.Info( this, "Started" );

      try
      {
        while ( !token.IsCancellationRequested )
        {
          try
          {
            await ProcessIterationAsync( token );
          }
          catch ( OperationCanceledException )
          {
            break;
          }
          catch ( Exception ex )
          {
            _context.Logger.Error( this, $"ProcessIterationAsync Error: {ex.GetFullMessage()}" );
          }

          var delay =
              Math.Max( ( ushort ) 5, _context.Settings.PollingInterval );

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
        _context.Logger.Error( this, $"Common AgentService Error: {ex.GetFullMessage()}" );
      }
      finally
      {
        _context.AgentStatusListener.ReportStatus( AgentState.Stopped );
      }

      _context.Logger.Info( this, "Stopped" );
    }

    async Task ProcessIterationAsync( CancellationToken token )
    {
      var pollingService = CoreFactory.CreatePollingService( _context );

      AgentResponse response =
          await pollingService.PollAsync( new AgentRequest() { AgentId = _context.Settings.HostUID }, token );

      _context.Logger.Info( this, $"Executing {response.Commands.Count} commands..." );
      _context.AgentStatusListener.ReportStatus( AgentState.Executing );

      // переносим текущие результаты на отправку
      var results = new List<CommandResult>();
      var currentCount = _postponedResults.Count;
      while ( results.Count < currentCount && _postponedResults.TryDequeue( out var cmdRes ) )
        results.Add( cmdRes );

      var dispatcher = new CommandDispatcher( CommandHandlerFactory.CreateDefault( _serviceContext, _context ), token );
      foreach ( var command in response.Commands )
      {
        var result =
            await dispatcher.ExecuteAsync( command );

        results.Add( result );
      }

      if ( results.Count > 0 )
      {
        await pollingService.ReportAsync(
            results,
            token );

        if ( results.Count > 0 )
          results.ForEach( r => _postponedResults.Enqueue( r ) );
      }
    }

    class AgentServiceContext( AgentService service ) : IAgentServiceContext
    {
      // особый случай - изменение настроек агента, локально, из команды. хост тоже может изменить настройки, но чтобы они вступили в силу, хост должен перезапустить агента. а локально агент может изменить настройки и продолжить работу с ними.
      public void UpdateSettings( IHostSettings settings )
      {
        service._context = new HostingContext { Settings = settings, Logger = service._context.Logger, AgentStatusListener = service._context.AgentStatusListener, HostShowMessage = service._context.HostShowMessage };

        service._context.Logger.Info( this, "Settings changed locally." );
      }

      // добавление результата выполнения команды в очередь на отправку на сервер для длительно выполняющихся команд. 
      public void AddCommandResult( CommandResult result )
      {
        service._postponedResults.Enqueue( result );
      }
    }

  }

}
