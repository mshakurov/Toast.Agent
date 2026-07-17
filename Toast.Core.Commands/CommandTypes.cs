namespace Toast.Core.Commands;

public static class CommandTypes
{
  static string[]? _all;
  public static string[] All => _all ??= [.. typeof( CommandTypes ).GetFields( System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public )
    .Select( p => p.GetValue( null )?.ToString() ).OfType<string>()];

  public static readonly string ShowMessage = "showMessage";
  public static readonly string ChangeSettings = "changeSettings";
  public static readonly string GetDeviceInfo = "getDeviceInfo";
}