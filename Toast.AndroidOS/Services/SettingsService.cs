using System;
using System.Text.Json;

using Android.App;
using Android.Content;

using Toast.AndroidOS.Models;
using Toast.Core.Interfaces;


namespace Toast.AndroidOS.Services;

public class SettingsService( string package_name, ILogger logger )
{
  object locker = new();

  // Имя файла настроек внутри приватной папки Android приложения
  private string PrefsFileName => $"{package_name}.settings";
  private const string SettingsKey = "host_settings_json";

  // Считывание настроек
  public HostSettings LoadSettings()
  {
    lock ( locker )
    {
      // 1. Получаем доступ к системным SharedPreferences (режим Private защищает данные от других приложений)
      var prefs = Application.Context.GetSharedPreferences( PrefsFileName, FileCreationMode.Private );

      if ( prefs == null )
      {
        LogError( "# Недоступны SharedPreferences" );

        // На случай, если что-то пошло не так
        return new HostSettings();
      }

      // 2. Достаем сохраненную JSON-строку
      string? json = prefs.GetString( SettingsKey, null );

      // 3. Если данных еще нет, возвращаем объект по умолчанию
      if ( string.IsNullOrWhiteSpace( json ) )
      {
        LogError( "# Не удалось считать настройки" );

        return new HostSettings();
      }

      try
      {
        // 4. Десериализуем строку обратно в объект C#
        return JsonSerializer.Deserialize<HostSettings>( json ) ?? new HostSettings();
      }
      catch ( Exception exc )
      {
        LogError( $"# Десериал настр: {exc.Message}{( exc.InnerException != null ? $", {exc.InnerException.Message}" : string.Empty )}{( exc.InnerException?.InnerException != null ? $", exc.InnerException.InnerException.Message" : string.Empty )}" );

        // На случай, если вы измените структуру класса в будущем
        return new HostSettings();
      }
    }
  }

  // Сохранение настроек
  public void SaveSettings( HostSettings settings )
  {
    if ( settings == null ) return;

    lock ( locker )
    {
      var prefs = Application.Context.GetSharedPreferences( PrefsFileName, FileCreationMode.Private );

      if ( prefs == null )
      {
        LogError( "# Недоступны SharedPreferences" );

        // На случай, если что-то пошло не так
        return;
      }

      // 1. Открываем транзакцию на редактирование
      using var editor = prefs.Edit();

      if ( editor == null )
      {
        LogError( "# Не редактируются SharedPreferences" );

        // На случай, если что-то пошло не так
        return;
      }

      try
      {
        // 2. Сериализуем объект в JSON
        string json = JsonSerializer.Serialize( settings );

        // 3. Записываем строку в память
        editor.PutString( SettingsKey, json );

        // 4. Применяем изменения асинхронно в фоне (не блокирует UI)
        editor.Apply();
      }
      catch ( Exception exc )
      {
        LogError( $"# Не сохран настр: {exc.Message}{( exc.InnerException != null ? $", {exc.InnerException.Message}" : string.Empty )}{( exc.InnerException?.InnerException != null ? $", exc.InnerException.InnerException.Message" : string.Empty )}" );
      }
    }
  }

  // Полное удаление настроек (при логауте или сбросе приложения)
  public void ClearSettings()
  {
    lock ( locker )
    {
      var prefs = Application.Context.GetSharedPreferences( PrefsFileName, FileCreationMode.Private );
      if ( prefs == null )
      {
        // На случай, если что-то пошло не так
        return;
      }
      using var editor = prefs.Edit();
      if ( editor == null )
      {
        // На случай, если что-то пошло не так
        return;
      }
      editor.Remove( SettingsKey );
      editor.Apply();
    }
  }

  void LogError( string error )
  {
    logger.Error( Application.Context, error );
    Android.Widget.Toast.MakeText( Application.Context, error, ToastLength.Long )?.Show();
  }
}

