namespace Toast.Core.Interfaces
{
  public interface IAgentService
  {
    Task ExecuteAsync( CancellationToken token );
  }
}
