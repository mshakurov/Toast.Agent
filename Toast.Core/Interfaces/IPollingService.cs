namespace Toast.Core.Interfaces
{
  public interface IPollingService
  {
    Task ExecuteAsync( CancellationToken token );
  }
}
