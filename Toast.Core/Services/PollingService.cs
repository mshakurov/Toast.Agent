using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Commands;
using Toast.Core.Interfaces;
using Toast.Core.Models;
using Toast.Core.Networking;

namespace Toast.Core.Services
{
  internal class PollingService : IPollingService
  {
    private readonly HostingContext _context;

    public PollingService( HostingContext context )
    {
      _context = context;
    }

    public Task<AgentResponse> PollAsync( AgentRequest request, CancellationToken token )
    {
      if ( _context.Settings.LastSuccessfulServerIndex > _context.Settings.Servers.Length - 1 )
        _context.Settings.LastSuccessfulServerIndex = 0;

      _context.Logger.Info( this, "Polling server..." );
      _context.AgentStatusListener.ReportStatus( AgentState.Polling );

      //_context.Settings.
      var client = new SecureClient( _context.Logger ).SecureDataClient;
      client.GetFromJsonAsync<List<TestDataItem>>( "api/data/commands" );

      return Task.FromResult( new AgentResponse() );
    }

    public async Task ReportAsync( IReadOnlyList<CommandResult> results, CancellationToken token )
    {
      _context.Logger.Info( this, $"Sending {results.Count}  answers ..." );
      _context.AgentStatusListener.ReportStatus( AgentState.Answering );

      await Task.CompletedTask;
    }
  }
}
