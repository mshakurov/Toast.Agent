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

    public DataController( ApplicationDbContext context )
    {
      this.context = context;
    }

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
    public async Task<IActionResult> GetCommands( [FromBody] AgentRequest request )
    {
      //List<AgentCommand> commands =
      //  [
      //    new () { Id = Guid.NewGuid(), Type = CommandTypes.ShowMessage,  JsonParameters = JsonSerializer.Serialize( new ShowMessageData { Title = "Hallow device!", Message  = $"From server! You are: {request.AgentId}", Duration = 11, WaitIfShow = false  } ) }
      //  ];

      var agentClient = await context.AgentClient.FindAsync( request.AgentId );
      if ( agentClient == null )
      {
        agentClient = context.AgentClient.Add( new Data.Models.AgentClient() { ClientId = request.AgentId } ).Entity;

        // 2. ОТПРАВЛЯЕМ ИЗМЕНЕНИЯ В БАЗУ ДАННЫХ
        // Только в этот момент EF Core сгенерирует SQL-команду INSERT и выполнит её в SQL Server
        await context.SaveChangesAsync();

        return Ok( new AgentResponse() );
      }
      else
      {
        var commandsFor = await context.AgentCommandFor.Where( ac => ac.Client != null && ac.Client.ClientId == request.AgentId ).ToListAsync();

        return Ok( new AgentResponse { Commands = commandsFor.Select( cf => cf.Command ).ToList() } );
      }
    }
  }
}
