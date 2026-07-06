
using Toast.Core.Interfaces;

namespace Toast.AndroidOS.Logging
{
  public sealed class AndroidLogger : ILogger
  {
    public void Debug( object source, string message )
    {
      Android.Util.Log.Debug( GetTag( source ), message );
    }

    public void Error( object source, string message )
    {
      Android.Util.Log.Error( GetTag( source ), message );
    }

    public void Error( object source, Exception exception )
    {
      Android.Util.Log.Error( GetTag( source ), exception.ToString() );
    }

    public void Info( object source, string message )
    {
      Android.Util.Log.Info( GetTag( source ), message );
    }

    public void Warning( object source, string message )
    {
      Android.Util.Log.Warn( GetTag( source ), message );
    }

    private static string GetTag( object source )
    {
      var tag = source?.GetType().Name ?? "Unknown";
      return tag/*.Length > 23 ? tag[(tag.Length - 23)..] : tag*/;
    }
  }
}
