using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Interfaces;

namespace Toast.Core.Services
{
  public static class CoreFactory
  {

    public static IPollingService CreatePollingService( ILogger logger ) => new PollingService( logger );

  }
}
