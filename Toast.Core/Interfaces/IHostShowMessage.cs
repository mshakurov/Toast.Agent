using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Interfaces
{
  public interface IHostShowMessage
  {
    void StartShowMessage( string message, string caption, int duration, Action<string?>? onResult );
  }
}
