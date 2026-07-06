namespace Toast.Core.Models;

public enum AgentState
{
  Starting,

  Waiting,

  Polling,

  Executing,

  Offline,

  Error,

  Stopping,

  Stopped
}
