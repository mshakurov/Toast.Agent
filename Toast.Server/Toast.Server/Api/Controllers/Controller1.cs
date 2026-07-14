using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

using Humanizer;

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
      => Ok( commandService.GetProtectedData( [new TestDataItem( 3, "Пользователь", User.FindFirstValue( ClaimTypes.NameIdentifier )?.ToString() ?? string.Empty )], GetInfo( this.ControllerContext ) ) );

    [HttpPost( "commands" )]
    public async Task<IActionResult> GetCommands( [FromBody] AgentRequest request )
      => Ok( await commandService.GetCommands( request, GetInfo( this.ControllerContext ) ) );

    [HttpPost( "results" )]
    public async Task<IActionResult> SetResults( [FromBody] AgentResult agentResult )
    {
      await commandService.SetResults( agentResult, GetInfo(this.ControllerContext) );
      return Ok();
    }

    public static CommandService.ClientInfo GetInfo( ControllerContext ControllerContext )
      =>
        new ( ControllerContext.HttpContext.Connection.RemoteIpAddress?.ToString(), ControllerContext.HttpContext.Connection.RemotePort, ControllerContext.HttpContext.Connection.LocalPort, ControllerContext.HttpContext.Features.Humanize(), DateTime.UtcNow);
  }
}
