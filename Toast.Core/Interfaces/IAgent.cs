namespace Toast.Core.Interfaces
{
  public interface IAgent
  {
    Task ExecuteAsync( CancellationToken token );
  }
}
