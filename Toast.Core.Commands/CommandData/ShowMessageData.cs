using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Commands.CommandData
{
  public class ShowMessageData
  {
    public string Message { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Duration { get; set; } = 0;
    public bool WaitIfShow { get; set; } = false;
  }
}
