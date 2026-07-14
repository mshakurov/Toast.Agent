using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Sockets;
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
        token.ThrowIfCancellationRequested();

        //var checkError = await CheckHostPort( server.s.HostURL!, server.s.Port, token );
        //if ( checkError != null )
        //{
        //  _context.Logger.Warning( this, $"# Connect to {server.s.HostURL}:{server.s.Port} failed ({server.s.GetKey()})." );
        //  continue;
        //}

        _context.Logger.Info( this, $"Polling server {server.s.GetKey()}..." );
        _context.AgentStatusListener.ReportStatus( AgentState.Polling );

        //var secureClient = new SecureClient( server.s.BaseUrl, server.s.LoginModel!, server.s.LastAuthToken, _context.Logger );

        var setsChanged = false;

        //List<Exception> exceptions = new();

        //AgentResponse? agentResponse = null;
        //try
        //{
        //  agentResponse = await GetAgentResponse( secureClient.SecureDataClient, request, token );
        //}
        //catch ( Exception ex )
        //{
        //  setsChanged = server.s.LastAuthToken != null;
        //  setsChanged = true;
        //  exceptions.Add( ex );

        //  if ( ex is UnauthorizedException exUnA )
        //  {
        //    secureClient = new SecureClient( server.s.BaseUrl, server.s.LoginModel!, null, _context.Logger );
        //    try
        //    {
        //      agentResponse = await GetAgentResponse( secureClient.SecureDataClient, request, token );
        //    }
        //    catch ( Exception ex2 )
        //    {
        //      exceptions.Add( ex2 );
        //    }
        //  }
        //}

        //if ( server.s.LastAuthToken != secureClient.LastAuthToken )
        //{
        //  server.s.LastAuthToken = secureClient.LastAuthToken;
        //  setsChanged = true;
        //}

        //if ( agentResponse != null )
        //  if ( _context.Settings.LastSuccessfulServerIndex != server.i )
        //  {
        //    _context.Settings.LastSuccessfulServerIndex = server.i;
        //    setsChanged = true;
        //  }

        //if ( setsChanged )
        //  _ = _context.Settings.Update();

        //if ( agentResponse != null )
        //  return agentResponse;

        var result = await TryProcess( server.s, ( client, token ) => GetAgentResponse( client, request, token ), token );

        if ( server.s.LastAuthToken != result.LastAuthToken )
        {
          server.s.LastAuthToken = result.LastAuthToken;
          setsChanged = true;
        }

        if ( result.result != null )
          if ( _context.Settings.LastSuccessfulServerIndex != server.i )
          {
            _context.Settings.LastSuccessfulServerIndex = server.i;
            setsChanged = true;
          }

        if ( setsChanged )
          _ = _context.Settings.Update();

        if ( result.result != null )
          return result.result;

        _context.Logger.Error( this, $"Ошибка запроса с сервера '{server.s.GetKey()}': {string.Join( ", ", result.exceptions.Select( ex => $"# {ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}" ) )}" );
      }

      return new AgentResponse();
    }

    public async Task<AgentResponse?> GetAgentResponse( HttpClient client, AgentRequest request, CancellationToken token )
    {
      //var iDbg = 1;
      //if (iDbg == 1)
      //{
      //  var srv = CoreFactory.CreateTestServerAuthorizedRequestService( "https://192.168.1.252", new LoginModel( "mshakurov@yandex.ru", "SuperPassword2026$" ), _context.Logger );

      //  return new AgentResponse();
      //}

      HttpResponseMessage response = await client.PostAsJsonAsync( "api/data/commands", request, token );

      token.ThrowIfCancellationRequested();

      if ( response.IsSuccessStatusCode )
      {
        var agentResponse = await response.Content.ReadFromJsonAsync<AgentResponse>( token );
        if ( agentResponse == null )
          throw new Exception( "Сервер вернул null" );
        return agentResponse;
      }
      else
      {
        // Обработка ошибок (например, 401 Unauthorized или 500 Server Error)
        var errorContent = await response.Content.ReadAsStringAsync( token );
        throw new Exception( $"Ошибка сервера: {response.StatusCode}. Детали: {errorContent}" );
      }
    }

    delegate Task<TResult> Getter<TResult>( HttpClient client, CancellationToken token );

    async Task<(TResult? result, AuthResponse? LastAuthToken, List<Exception> exceptions)> TryProcess<TResult>(RemoteServer remoteServer, Getter<TResult> getter, CancellationToken token)
    {
      var secureClient = new SecureClient( remoteServer.BaseUrl, remoteServer.LoginModel!, remoteServer.LastAuthToken, _context.Logger );

      try
      {
        List<Exception> exceptions = new();

        TResult? result = default;
        try
        {
          result = await getter( secureClient.SecureDataClient, token );
        }
        catch ( Exception ex )
        {
          exceptions.Add( ex );

          if ( ex is UnauthorizedException exUnA )
          {
            secureClient.Dispose();
            secureClient = new SecureClient( remoteServer.BaseUrl, remoteServer.LoginModel!, null, _context.Logger );
            try
            {
              result = await getter( secureClient.SecureDataClient, token );
            }
            catch ( Exception ex2 )
            {
              exceptions.Add( ex2 );
            }
          }
        }

        return (result, secureClient.LastAuthToken, exceptions);
      }
      finally
      {
        secureClient.Dispose();
      }
    }

    public async Task ReportAsync( List<CommandResult> results, CancellationToken token )
    {
      _context.Logger.Info( this, $"Sending {results.Count}  answers ..." );
      _context.AgentStatusListener.ReportStatus( AgentState.Answering );

      if ( _context.Settings.LastSuccessfulServerIndex > _context.Settings.Servers.Length - 1 )
        _context.Settings.LastSuccessfulServerIndex = 0;

      var servers = _context.Settings.Servers.Select( ( s, i ) => (s, i: ( ushort ) i) ).Take( ushort.MaxValue ).Where( s => !string.IsNullOrWhiteSpace( s.s.HostURL ) && s.s.LoginModel != null && !string.IsNullOrWhiteSpace( s.s.LoginModel.Email ) ).OrderBy( s => s.i == _context.Settings.LastSuccessfulServerIndex ? 1 : 2 ).ThenBy( s => s.i ).ToArray();
      if ( servers.Length == 0 )
      {
        throw new Exception( $"# Не найден ни один правильно настроенный сервер" );
      }

      AgentResult agentResult = new AgentResult { AgentId = _context.Settings.HostUID, Results = results };
      foreach ( var server in servers )
      {
        _context.Logger.Info( this, $"Answering to server {server.s.GetKey()}..." );
        _context.AgentStatusListener.ReportStatus( AgentState.Answering );

        var setsChanged = false;

        var result = await TryProcess( server.s, async ( client, token ) =>
        {
          HttpResponseMessage response = await client.PostAsJsonAsync( "api/data/results", agentResult, token );

          if ( response.IsSuccessStatusCode )
          {
            results.Clear();

            return true;
          }
          else
          {
            // Обработка ошибок (например, 401 Unauthorized или 500 Server Error)
            var errorContent = await response.Content.ReadAsStringAsync( token );
            throw new Exception( $"Ошибка сервера: {response.StatusCode}. Детали: {errorContent}" );
          }
        }, token );

        if ( server.s.LastAuthToken != result.LastAuthToken )
        {
          server.s.LastAuthToken = result.LastAuthToken;
          setsChanged = true;
        }

        if ( result.result )
          if ( _context.Settings.LastSuccessfulServerIndex != server.i )
          {
            _context.Settings.LastSuccessfulServerIndex = server.i;
            setsChanged = true;
          }

        if ( setsChanged )
          _ = _context.Settings.Update();

        if ( result.result )
          return;
      }

      await Task.CompletedTask;
    }

    public async Task<string?> CheckHostPort( string hostUrl, int port, CancellationToken token )
    {
      TcpClient client = new();
      try
      {
        var taskConnect = client.ConnectAsync( hostUrl, port, token ).AsTask();
        await Task.WhenAny( client.ConnectAsync( hostUrl, port, token ).AsTask(), Task.Delay( TimeSpan.FromSeconds( 3 ), token ) );
        return taskConnect.IsCompleted ? null : $"Timeout";
      }
      catch ( Exception ex )
      {
        return $"{ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}";
      }
      finally
      {
        try { client.Close(); } catch { }
        try { client.Dispose(); } catch { }
      }
    }
  }
}
