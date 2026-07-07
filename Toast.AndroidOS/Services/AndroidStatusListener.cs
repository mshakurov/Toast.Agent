using Toast.AndroidOS.Notifications;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.AndroidOS.Services
{
  internal sealed class AndroidStatusListener : IHostStatusListener
  {
    private readonly Context _context;
    private readonly ILogger? _logger;
    private readonly Task _notifyTask;
    private readonly CancellationToken _token;
    private readonly State _pendingState;

    public AndroidStatusListener( Context context, ILogger? logger, CancellationToken token )
    {
      _context = context;
      _logger = logger;
      _pendingState = new();
      _token = token;
      _notifyTask = Task.Factory.StartNew( NotifyLastStatus );
    }

    private void NotifyLastStatus()
    {
      while ( true )
      {
        try { _token.WaitHandle.WaitOne( 1000 ); } catch { }

        if ( _token.IsCancellationRequested ) break;

        DoNotify( _pendingState.Get() );
      }
    }

    private void DoNotify( State state )
    {
      if ( state.PendingStatus == null )
        return;
      try
      {
        NotificationHelper.UpdateNotification( _context, state.PendingStatus );
      }
      catch ( Exception ex )
      {
        _logger?.Error( _context, $"Failed to update notification for status (({state.Count}){state.PendingStatus}): {ex.Message},{ex.InnerException?.Message},{ex.InnerException?.InnerException?.Message}" );
      }
    }

    public void ReportStatus( AgentStatus status )
    {
      //_logger?.Debug( this, $"> {_pendingState.Count}){_pendingState.PendingStatus?.State}|{_pendingState.PendingStatus?.Details} -> ReportStatus({status.State}|{status.Details})" );

      _pendingState.Add( status );

      //_logger?.Debug( this, $"< {_pendingState.Count}){_pendingState.PendingStatus?.State}|{_pendingState.PendingStatus?.Details}" );
    }

    public void ReportStatus( AgentState state ) => ReportStatus( AgentStatus.FromState( state ) );

    class State
    {
      object locker = new();

      public long Count { get; private set; }
      public AgentStatus? PendingStatus { get; private set; }

      public void Add( AgentStatus status )
      {
        lock ( locker )
        {
          Count += 1;
          PendingStatus = new AgentStatus { State = status.State, Details = $"({Count} at {status.Timestamp:HH:mm:ss})" + ( status.Details != null ? $" {status.Details}" : string.Empty ), Timestamp = status.Timestamp };
        }
      }

      public State Get()
      {
        lock ( locker )
          return new State { Count = Count, PendingStatus = PendingStatus };
      }
    }
  }
}
