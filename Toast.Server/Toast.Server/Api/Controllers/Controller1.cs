using System.Security.Claims;
using System.Text.Json;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Toast.Core.Commands;
using Toast.Core.Commands.CommandData;

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
            new(2, "Товар 2", "Секретное значение Б"),
            new(3, "Пользователь", User.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() ?? string.Empty),
        };
      return Ok( secureData );
    }

    [HttpPost( "commands" )]
    public IActionResult GetCommands( [FromBody] AgentRequest request )
    {
      List<AgentCommand> commands =
        [
          new () { Id = Guid.NewGuid(), Type = CommandTypes.ShowMessage,  JsonParameters = JsonSerializer.Serialize( new ShowMessageData { Title = "Hallow device!", Message  = $"From server! You are: {request.AgentId}", Duration = 11, WaitIfShow = false  } ) }
        ];

      return Ok( new AgentResponse { Commands = commands } );
    }
  }
}
