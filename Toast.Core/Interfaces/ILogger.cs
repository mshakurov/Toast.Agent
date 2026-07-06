using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Interfaces
{
  public interface ILogger
  {
    void Debug( object source, string message );

    void Info( object source, string message );

    void Warning( object source, string message );

    void Error( object source, string message );

    void Error( object source, Exception exception );
  }
}
