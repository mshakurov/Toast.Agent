using System.Text.Json;

namespace Toast.Server.Api
{
  public static class Extensions
  {
    public static TItem? Clone<TItem>( this TItem? item ) => item == null ? default : JsonSerializer.Deserialize<TItem>( JsonSerializer.Serialize( item ) );

    public static TItem CloneEx<TItem>( this TItem item ) => JsonSerializer.Deserialize<TItem>( JsonSerializer.Serialize( item ) ) ?? throw new Exception( $"Clone error" );
  }
}
