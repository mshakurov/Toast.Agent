using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AndroidX.Core.App;

using Toast.AndroidOS.Notifications;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.AndroidOS.Services
{
  internal sealed class AndroidStatusListener : IAgentStatusListener
  {
    private readonly Context _context;
    private readonly ILogger? _logger;

    public AndroidStatusListener( Context context, ILogger? logger )
    {
      _context = context;
      _logger = logger;
    }

    public void ReportStatus( AgentStatus status )
    {
      try
      {
        NotificationHelper.UpdateNotification( _context, status );
      }
      catch ( Exception ex )
      {
        _logger?.Error( _context, $"Failed to update notification for status (state: {status.State}): {ex.Message},{ex.InnerException?.Message},{ex.InnerException?.InnerException?.Message}" );
      }
    }

    public void ReportStatus( AgentState state )
    {
      try
      {
        NotificationHelper.UpdateNotification( _context, AgentStatus.FromState( state ) );
      }
      catch ( Exception ex )
      {
        _logger?.Error( _context, $"Failed to update notification for state {state}: {ex.Message},{ex.InnerException?.Message},{ex.InnerException?.InnerException?.Message}" );
      }
    }
  }
}
