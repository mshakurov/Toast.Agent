using System.Net.Http.Headers;

using Toast.Core.Services;

public class JwtAuthorizationHandler : DelegatingHandler
{
  private readonly AuthService _authService;

  public JwtAuthorizationHandler( AuthService authService )
  {
    _authService = authService;
  }

  protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
  {
    // 1. Получаем свежий токен (класс AuthService сам решит, обновить его или взять из кэша)
    string token = await _authService.GetValidTokenAsync();

    // 2. Внедряем токен в заголовок запроса
    request.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", token );

    // 3. Отправляем запрос дальше по цепочке
    return await base.SendAsync( request, cancellationToken );
  }
}
