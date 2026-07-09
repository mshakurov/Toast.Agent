using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Toast.Core.Commands;
using Toast.Server.Data;



namespace Toast.Server.Api.Controllers;

[ApiController]
[Route( "api/[controller]" )]
public class AuthController : ControllerBase
{
  private readonly SignInManager<ApplicationUser> _signInManager;
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly IConfiguration _configuration;

  public AuthController( SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IConfiguration configuration )
  {
    _signInManager = signInManager;
    _userManager = userManager;
    _configuration = configuration;
  }

  [HttpPost( "login" )]
  public async Task<IActionResult> Login( [FromBody] LoginModel model )
  {
    // 1.Ищем пользователя по Email
    var user = await _userManager.FindByEmailAsync( model.Email );
    if ( user == null )
    {
      return Unauthorized( new { Message = "Неверный логин или пароль" } );
    }

    // 2. Проверяем пользователя через встроенный Identity
    var result = await _signInManager.CheckPasswordSignInAsync(
        user,
        model.Password,
        lockoutOnFailure: false );

    if ( !result.Succeeded )
    {
      return Unauthorized( new { Message = "Неверный логин или пароль" } );
    }

    // 3. Если успешно, генерируем JWT-токен
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.UTF8.GetBytes( _configuration["Jwt:Key"]! );
    var expires = DateTime.UtcNow.AddDays( 7 ); // Срок действия токена

    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity( new[]
      {
        new Claim(ClaimTypes.NameIdentifier, user.Id), // ТЕПЕРЬ ID ТАКЖЕ ЗАКЛЮЧЕН В ТОКЕНЕ!
        new Claim( ClaimTypes.Name, model.Email ),
      } ),
      Expires = expires,
      Issuer = _configuration["Jwt:Issuer"],
      Audience = _configuration["Jwt:Audience"],
      SigningCredentials = new SigningCredentials( new SymmetricSecurityKey( key ), SecurityAlgorithms.HmacSha256Signature )
    };

    var token = tokenHandler.CreateToken( tokenDescriptor );

    return Ok( new AuthResponse( tokenHandler.WriteToken( token ), expires ) );
  }
}
