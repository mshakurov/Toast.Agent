namespace Toast.Core.Models;

public enum AgentState
{
  Starting,

  Waiting,

  Polling,

  Executing,

  Answering,

  Offline,

  Error,

  Stopping,

  Stopped
}
