using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using Toast.Core.Commands;

namespace Toast.Core.Services;

public class AuthService
{
  private readonly HttpClient _authClient;
  private AuthResponse? _cachedAuthToken;

  // Сюда можно зашить дефолтные учетки или передавать динамически
  private readonly LoginModel _credentials;

  public AuthResponse? LastAuthToken => _cachedAuthToken;

  public AuthService( LoginModel credentials, AuthResponse? cachedAuthToken, HttpClient authClient )
  {
    _authClient = authClient;
    _credentials = credentials;
    _cachedAuthToken = cachedAuthToken;
  }

  public async Task<AuthResponse> GetValidTokenAsync()
  {
    // Если токен есть и он еще действует (с запасом в 1 минуту), возвращаем его
    if ( _cachedAuthToken != null && !string.IsNullOrEmpty( _cachedAuthToken.Token ) && _cachedAuthToken.Expiration > DateTime.UtcNow.AddMinutes( 1 ) )
    {
      return _cachedAuthToken;
    }

    // Иначе — прозрачно делаем запрос на авторизацию
    var response = await _authClient.PostAsJsonAsync( "api/auth/login", _credentials );

    if ( response.IsSuccessStatusCode )
    {
      var authData = await response.Content.ReadFromJsonAsync<AuthResponse>();
      if ( authData != null )
      {
        _cachedAuthToken = authData;
        return _cachedAuthToken;
      }
      else
        throw new HttpRequestException( "Ошибка автоматической авторизации устройства (Ответ пустой)." );
    }

    throw new HttpRequestException( "Ошибка автоматической авторизации устройства." );
  }
}
