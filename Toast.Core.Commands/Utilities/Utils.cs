using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Toast.Core.Utilities;

public static class Utils
{
  public delegate string? DumpPropsFormatterDelegate( PropertyInfo propertyInfo, object? properyValue );

  private static object? GetPropertyValue( PropertyInfo p, object? o )
  {
    try
    {
      return p.GetValue( o, null );
    }
    catch ( Exception ex )
    {
      return ex;
    }
  }

  /// <summary>
  /// Печатает значения полей через заданный разделитель, с помощью заданного форматтера.
  /// </summary>
  /// <param name="obj">Объект</param>
  /// <param name="delimiter">Разделитель</param>
  /// <param name="formatter">Функция форматирования. По умолчанию - просто значение</param>
  /// <returns>Распечатка значений полей объекта</returns>
  public static string DumpProps( this object obj, string delimiter = ", ", DumpPropsFormatterDelegate? formatter = null )
  {
    if ( obj == null )
      return "[null]";
    var type = obj.GetType();
    if ( type.IsPrimitive || type.IsValueType || type == typeof( string ) || type == typeof( DateTime ) )
      return obj.ToString()!;

    if ( formatter == null )
      formatter = ( pi, val ) => $"{val}";

    return string.Join( delimiter, type.GetProperties().Where( p => p.CanRead && p.GetIndexParameters().Length == 0 )
      .Select( p => formatter( p, GetPropertyValue( p, obj ) ) ).OfType<string>() );
  }

  /// <summary>
  /// Печатает значения полей в формате 'Имя [тип]: Значение'
  /// </summary>
  /// <param name="obj">Объект</param>
  /// <returns>Распечатка значений полей объекта</returns>
  public static string DumpPropsFullNewLine( this object obj )
    => obj.DumpProps( Environment.NewLine, ( p, v ) => $"{p.Name} [{p.PropertyType.Name}]: {v}" );

  public static string PadOrTrimRight( this string str, int len, char c )
  {
    if ( str.Length > len )
      return str.Substring( 0, len );
    else
      return str.PadRight( len, c );
  }

  public static string? GetFullMessage( this Exception exc, string delimiter = ", ", HashSet<Exception>? hash = null )
  {
    if ( exc == null )
      return null;
    if ( hash == null )
      hash = new();
    if ( hash.Contains( exc ) )
      return $"INTERNAL LOOP: {exc}";
    hash.Add( exc );
    var log = new List<string?>();
    if ( exc is AggregateException aggExc )
      foreach ( var iexc in aggExc.InnerExceptions )
      {
        var msg = iexc.GetFullMessage( delimiter, hash );
        if ( msg != null )
          log.Add( msg );
      }
    else
    {
      log.Add( exc.Message );
      if ( exc.InnerException != null )
      {
        var msg = exc.GetFullMessage( delimiter, hash );
        log.Add( msg );
      }
    }
    return log.Count > 0 ? string.Join( delimiter, log ) : null;
  }

  public static string? NullIfWhiteSpace( this string? str ) => string.IsNullOrWhiteSpace( str ) ? null : str;

}
