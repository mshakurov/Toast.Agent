using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Commands;
using Toast.Core.Interfaces;
using Toast.Core.Services;

namespace Toast.Core.Networking
{
  internal class SecureClient : IDisposable
  {
    HttpClient baseClient;
    HttpClient _secureDataClient;
    public HttpClient SecureDataClient
    {
      get => _secureDataClient;
      set
      {
        try { _secureDataClient?.Dispose(); } catch { }
        _secureDataClient = value;
      }
    }
    private readonly ILogger logger;
    private readonly AuthService authService;

    public AuthResponse? LastAuthToken => authService.LastAuthToken;

    public SecureClient( string baseServerUrl, LoginModel credentials, AuthResponse? lastAuthToken, Interfaces.ILogger logger )
    {
      this.logger = logger;
      HttpClientHandler httpClientHandler = new()
      {
        //SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
      };
      httpClientHandler.ServerCertificateCustomValidationCallback += ( _1, _2, _3, _4 ) => true;

      // 1. Клиент чисто для аутентификации (ему не нужны заголовки)
      baseClient = new HttpClient( httpClientHandler ) { BaseAddress = new Uri( baseServerUrl ) };
      authService = new AuthService( credentials, lastAuthToken, baseClient );

      // 2. Собираем защищенный клиент, передавая ему наш кастомный Handler
      var jwtHandler = new JwtAuthorizationHandler( authService )
      {
        InnerHandler = httpClientHandler // Базовый сетевой обработчик Android
      };

      // ЭТИМ клиентом мы пользуемся во всем Android-приложении для работы с данными
      _secureDataClient = new HttpClient( jwtHandler )
      {
        BaseAddress = new Uri( baseServerUrl )
      };
    }

    public void Dispose()
    {
      baseClient.Dispose();
      _secureDataClient.Dispose();
    }
  }
}
