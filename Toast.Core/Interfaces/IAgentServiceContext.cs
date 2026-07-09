using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Commands;

namespace Toast.Core.Interfaces
{
  internal interface IAgentServiceContext
  {
    void UpdateSettings( IHostSettings settings );

    void AddCommandResult( CommandResult result );
  }
}
