using Toast.Core.Interfaces;

namespace Toast.Core.Logging
{
  internal class ConsoleLogger : ILogger
  {
    public void Debug( object source, string message )
    {
      Console.WriteLine( $"DEBUG: {source?.GetType().Name ?? "Unknown"} - {message}" );
    }

    public void Error( object source, string message )
    {
      Console.WriteLine( $"ERROR: {source?.GetType().Name ?? "Unknown"} - {message}" );
    }

    public void Error( object source, Exception exception )
    {
      Console.WriteLine( $"ERROR: {source?.GetType().Name ?? "Unknown"} - {exception}" );
    }

    public void Info( object source, string message )
    {
      Console.WriteLine( $"INFO: {source?.GetType().Name ?? "Unknown"} - {message}" );
    }

    public void Warning( object source, string message )
    {
      Console.WriteLine( $"WARNING: {source?.GetType().Name ?? "Unknown"} - {message}" );
    }
  }
}
