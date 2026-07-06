using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Interfaces
{
  public interface IPollingService
  {
    Task ExecuteAsync( CancellationToken token );
  }
}
