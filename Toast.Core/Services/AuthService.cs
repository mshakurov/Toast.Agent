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
  private string? _cachedToken;
  private DateTime _tokenExpiration;

  // Сюда можно зашить дефолтные учетки или передавать динамически
  private readonly LoginModel _credentials = new( "mshakurov@yandex.ru", "VorgeN2010$" );

  public AuthService( HttpClient authClient )
  {
    _authClient = authClient;
  }

  public async Task<string> GetValidTokenAsync()
  {
    // Если токен есть и он еще действует (с запасом в 1 минуту), возвращаем его
    if ( !string.IsNullOrEmpty( _cachedToken ) && _tokenExpiration > DateTime.UtcNow.AddMinutes( 1 ) )
    {
      return _cachedToken;
    }

    // Иначе — прозрачно делаем запрос на авторизацию
    var response = await _authClient.PostAsJsonAsync( "api/auth/login", _credentials );

    if ( response.IsSuccessStatusCode )
    {
      var authData = await response.Content.ReadFromJsonAsync<AuthResponse>();
      if ( authData != null )
      {
        _cachedToken = authData.Token;
        _tokenExpiration = authData.Expiration;
        return _cachedToken;
      }
      else
        throw new HttpRequestException( "Ошибка автоматической авторизации устройства (Ответ пустой)." );
    }

    throw new HttpRequestException( "Ошибка автоматической авторизации устройства." );
  }
}
