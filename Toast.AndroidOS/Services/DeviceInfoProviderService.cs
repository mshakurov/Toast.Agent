using Android.App;
using Android.Provider;

using Toast.Core.Utilities;

namespace Toast.AndroidOS.Services;

public static class DeviceInfoProviderService
{
  static string? _deviceUniqueIdentifier;
  
  
  public static string DeviceUniqueIdentifier => _deviceUniqueIdentifier ??= GetDeviceUniqueIdentifier();

  public static string GetDeviceUniqueIdentifier()
  {
    try
    {
      // Получаем контекст текущего приложения (работает везде: в Activity, Service и обычных классах)
      var context = Application.Context;

      // Запрашиваем ANDROID_ID из системных настроек Secure
      string? androidId = Settings.Secure.GetString( context.ContentResolver, Settings.Secure.AndroidId );

      // На случай, если что-то пошло не так (крайне редко), возвращаем дефолтное значение
      return androidId ?? "unknown_device_id";
    }
    catch ( Exception ex )
    {
      return $"# Can't read 'Settings.Secure.AndroidId': {ex.GetFullMessage()}. {Guid.NewGuid()}";
    }
  }
}
