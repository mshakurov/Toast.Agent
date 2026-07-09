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
using Toast.Core.Utilities;

namespace Toast.Core.Services
{
  internal class PollingService : IPollingService
  {
    private readonly HostingContext _context;

    public PollingService( HostingContext context )
    {
      _context = context;
    }

    public async Task<AgentResponse> PollAsync( AgentRequest request, CancellationToken token )
    {
      if ( _context.Settings.LastSuccessfulServerIndex > _context.Settings.Servers.Length - 1 )
        _context.Settings.LastSuccessfulServerIndex = 0;

      var servers = _context.Settings.Servers.Select( ( s, i ) => (s, i: ( ushort ) i) ).Take( ushort.MaxValue ).Where( s => !string.IsNullOrWhiteSpace( s.s.HostURL ) && s.s.LoginModel != null && !string.IsNullOrWhiteSpace( s.s.LoginModel.Email ) ).OrderBy( s => s.i == _context.Settings.LastSuccessfulServerIndex ? 1 : 2 ).ThenBy( s => s.i ).ToArray();
      if ( servers.Length == 0 )
      {
        throw new Exception( $"# Не найден ни один правильно настроенный сервер" );
      }

      foreach ( var server in servers )
      {
        _context.Logger.Info( this, $"Polling server {server.s.GetKey()}..." );
        _context.AgentStatusListener.ReportStatus( AgentState.Polling );

        var secureClient = new SecureClient( server.s.BaseUrl, server.s.LoginModel!, server.s.LastAuthToken, _context.Logger );

        var setsChanged = false;

        List<Exception> exceptions = new();

        AgentResponse? agentResponse = null;
        try
        {
          agentResponse = await GetAgentResponse( secureClient.SecureDataClient, request, token );
        }
        catch ( Exception ex )
        {
          secureClient = new SecureClient( server.s.BaseUrl, server.s.LoginModel!, null, _context.Logger );

          server.s.LastAuthToken = null;
          setsChanged = true;
          exceptions.Add( ex );

          try
          {
            agentResponse = await GetAgentResponse( secureClient.SecureDataClient, request, token );
          }
          catch ( Exception ex2 )
          {
            exceptions.Add( ex2 );
          }
        }

        if ( server.s.LastAuthToken != secureClient.LastAuthToken )
        {
          server.s.LastAuthToken = secureClient.LastAuthToken;
          setsChanged = true;
        }

        if ( agentResponse != null )
          if ( _context.Settings.LastSuccessfulServerIndex != server.i )
          {
            _context.Settings.LastSuccessfulServerIndex = server.i;
            setsChanged = true;
          }

        if ( setsChanged )
          _ = _context.Settings.Update();

        if ( agentResponse != null )
          return agentResponse;
      }

      return new AgentResponse();
    }

    public async Task<AgentResponse?> GetAgentResponse( HttpClient client, AgentRequest request, CancellationToken token )
    {
      return await client.GetFromJsonAsync<AgentResponse>( "api/data/commands", token );
    }

    public async Task ReportAsync( IReadOnlyList<CommandResult> results, CancellationToken token )
    {
      _context.Logger.Info( this, $"Sending {results.Count}  answers ..." );
      _context.AgentStatusListener.ReportStatus( AgentState.Answering );

      await Task.CompletedTask;
    }
  }
}
