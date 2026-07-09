using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Toast.Core.Commands;

namespace Toast.Server.Api.Controllers
{
  [ApiController]
  [Route( "api/[controller]" )]
  //[Authorize] // Защищает все эндпоинты контроллера
  [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
  public class DataController : ControllerBase
  {
    [HttpGet( "items" )]
    public IActionResult GetProtectedData()
    {
      var secureData = new List<TestDataItem>
        {
            new(1, "Товар 1", "Секретное значение А"),
            new(2, "Товар 2", "Секретное значение Б")
        };
      return Ok( secureData );
    }
  }
}
