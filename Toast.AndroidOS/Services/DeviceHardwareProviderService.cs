using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.OS;

namespace Toast.AndroidOS.Services;

public static class DeviceHardwareProviderService
{
  public static string GetDeviceHumanReadableName()
  {
    // 1. Производитель (например: Samsung, Xiaomi, Google, LGE)
    try
    {
      string? manufacturer = Build.Manufacturer;

      // 2. Бренд / Торговая марка (например: samsung, POCO, google)
      string? brand = Build.Brand;

      // 3. Рыночное название модели или код (например: SM-G998B, POCO X3 Pro, Pixel 7)
      string? model = Build.Model;

      return $"{brand} {model} ({manufacturer}, {Build.Device}, {Build.Hardware}, {Build.Host}, {Build.Type})";
    }
    catch ( Exception ex )
    {
      return $"# Can't read 'Build.Manufacturer': {ex.Message}";
    }
  }
}
