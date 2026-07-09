using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Models
{
  internal class UnauthorizedException : Exception
  {
    public readonly string Reason;
    public UnauthorizedException(string reason) : base(reason) 
    { 
      Reason = reason;
    }
  }
}
