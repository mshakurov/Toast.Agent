using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Models;

namespace Toast.Core.Interfaces
{
  public interface IAgentStatusListener
  {
    /// <summary>
    /// Метод вызывается для информирования об изменении состояния агента.
    /// </summary>
    /// <param name="status">Новое состояние агента.</param>
    void ReportStatus( AgentStatus status );

    /// <summary>
    /// Метод вызывается для информирования об изменении состояния агента.
    /// </summary>
    /// <param name="state">Новое состояние агента.</param>
    void ReportStatus( AgentState state );
  }
}
