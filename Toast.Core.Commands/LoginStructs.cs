using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Commands
{
  public record LoginModel( string Email, string Password );
  public record AuthResponse( string Token, DateTime Expiration );
  public record DataItem( int Id, string Name, string Value );
}
