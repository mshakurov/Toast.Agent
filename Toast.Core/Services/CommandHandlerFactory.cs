using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Commands;
using Toast.Core.Interfaces;
using Toast.Core.Models;

namespace Toast.Core.Services
{
  internal class CommandHandlerFactory
  {
    public static IReadOnlyList<ICommandHandler> CreateDefault(HostingContext context)
    {
      return
      [
          new ShowMessageCommandHandler(context),
      ];
    }
  }
}
