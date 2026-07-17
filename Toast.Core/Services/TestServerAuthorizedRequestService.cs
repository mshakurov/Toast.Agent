using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Commands;
using Toast.Core.Utilities;
using Toast.Core.Interfaces;

namespace Toast.Core.Services
{
  internal class TestServerAuthorizedRequestService: ITestServerAuthorizedRequestService
  {
    private readonly HttpClient _secureClient;
    private readonly ILogger _logger;

    public TestServerAuthorizedRequestService( HttpClient secureClient, ILogger logger )
    {
      _secureClient = secureClient; // Передаем сюда наш настроенный secureDataClient
      _logger = logger;
    }

    public async Task<(List<TestDataItem> Items, string? Exception)> LoadItemsFromServerAsync()
    {
      try
      {
        // Архитектурно чистый вызов. О JWT, логине и шифровании TLS заботится HttpClient!
        var items = await _secureClient.GetFromJsonAsync<List<TestDataItem>>( "api/data/items" );
        return (items ?? [], null);
      }
      catch ( Exception ex )
      {
        // Обработка ошибок сети
        _logger.Error(this, $"Ошибка загрузки данных: {ex.GetFullMessage()}" );

        return ([], $"{ex.Message}|{ex.InnerException?.Message}|{ex.InnerException?.InnerException?.Message}");
      }
    }
  }
}
