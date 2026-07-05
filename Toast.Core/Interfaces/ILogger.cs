using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Interfaces
{
  public interface ILogger
  {
    void Info( string message );

    void Warning( string message );

    void Error( Exception exception );
  }
}
