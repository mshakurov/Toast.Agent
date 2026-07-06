using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Toast.Server.Api.Controllers
{
  [ApiController]
  [Route( "api/[controller]" )]
  [Authorize] // Защищает все эндпоинты контроллера
  public class DataController : ControllerBase
  {
    [HttpGet( "items" )]
    public IActionResult GetSharedData()
    {
      var data = new { Message = "Это защищенные данные для Android", Date = DateTime.UtcNow };
      return Ok( data ); // Автоматически сериализует в JSON
    }
  }
}
