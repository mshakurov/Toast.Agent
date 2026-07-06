
using Toast.Core.Interfaces;

namespace Toast.AndroidOS.Logging
{
  public sealed class AndroidLogger : ILogger
  {
    string system;
    public AndroidLogger( string system ) { this.system = system; }
    
    public void Debug( object source, string message )
    {
      Android.Util.Log.Debug( GetTag( source ), GetMessage(source, message) );
    }

    public void Error( object source, string message )
    {
      Android.Util.Log.Error( GetTag( source ), GetMessage( source, message ) );
    }

    public void Error( object source, Exception exception )
    {
      Android.Util.Log.Error( GetTag( source ), GetMessage( source, exception.ToString() ) );
    }

    public void Info( object source, string message )
    {
      Android.Util.Log.Info( GetTag( source ), GetMessage( source, message ) );
    }

    public void Warning( object source, string message )
    {
      Android.Util.Log.Warn( GetTag( source ), GetMessage( source, message ) );
    }

    private string GetTag( object source )
    {
      var tag = system;
      return tag/*.Length > 23 ? tag[(tag.Length - 23)..] : tag*/;
    }

    private string GetMessage( object source, string message ) => $"{source?.GetType().Name ?? "Unknown"}|{message}";

  }
}
