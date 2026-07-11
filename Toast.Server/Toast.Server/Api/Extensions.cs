using System.Text.Json;

namespace Toast.Server.Api
{
  public static class Extensions
  {
    public static TItem? Clone<TItem>( this TItem? item ) => item == null ? default : JsonSerializer.Deserialize<TItem>( JsonSerializer.Serialize( item ) );

    public static TItem CloneEx<TItem>( this TItem item ) => JsonSerializer.Deserialize<TItem>( JsonSerializer.Serialize( item ) ) ?? throw new Exception( $"Clone error" );

    public static string FormatValue( this object? obj )
    {
      if ( obj == null )
        return $"{obj}";

      var type = obj.GetType();
      if ( type == typeof( string ) || type.IsPrimitive || type.IsValueType )
        return $"{obj}";

      object?[] arr;
      var collEn = obj as System.Collections.IEnumerable;
      if ( collEn != null )
        arr = collEn.OfType<object?>().ToArray();
      else
      {
        var collEnO = ( obj as IEnumerable<object?> ) ?? ( obj as IEnumerable<object> );
        if ( collEnO != null )
          arr = collEnO.ToArray();
        else
          return $"{obj}";
      }
      return $"({arr.Length}{( arr.Length > 20 ? "( 1st 20)" : string.Empty )}): {string.Join( ", ", arr.Take( 20 ) )}";
    }
  }
}
