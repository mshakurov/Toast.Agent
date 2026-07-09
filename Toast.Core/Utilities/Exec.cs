using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Utilities
{
  public static class Exec
  {
    public static TResult? TryGet<TResult>( Func<TResult?> getter, Func<Exception, TResult?>? onException )
    {
      try
      {
        return getter();
      }
      catch ( Exception ex )
      {
        if ( onException != null )
          return onException( ex );
        return default;
      }
    }
  }
}
