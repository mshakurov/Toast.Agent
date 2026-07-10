using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Toast.Core.Commands;
using Toast.Core.Commands.CommandData;
using Toast.Server.Data;

namespace Toast.Server.Api.Controllers
{
  [ApiController]
  [Route( "api/[controller]" )]
  //[Authorize] // Защищает все эндпоинты контроллера
  [Authorize( AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
  public class DataController : ControllerBase
  {
    private readonly ApplicationDbContext context;
    private readonly CommandService commandService;

    public DataController( ApplicationDbContext context, CommandService commandService )
    {
      this.context = context;
      this.commandService = commandService;
    }

    [HttpGet( "items" )]
    public IActionResult GetProtectedData() 
      => Ok( commandService.GetProtectedData( new TestDataItem( 3, "Пользователь", User.FindFirstValue( ClaimTypes.NameIdentifier )?.ToString() ?? string.Empty ) ) );

    [HttpPost( "commands" )]
    public async Task<IActionResult> GetCommands( [FromBody] AgentRequest request )
      => Ok( await commandService.GetCommands( request ) );
  }
}
