using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Toast.Core.Interfaces;

namespace Toast.AndroidOS.Activities
{
  internal class PermissionCheckHelper( Activity owner, ILogger? logger, TextView? textView, CancellationToken token )
  {
    private readonly ILogger? _logger = logger;
    private readonly TextView? _textView = textView;
    private readonly CancellationToken _token = token;
    private readonly Activity _owner = owner;
    private volatile Task? _checkTask;

    private int requestPermissionPostNotifyCount = 0;
    private int requestPermissionCheckNetworkCount = 0;

    public bool CheckPermissions() => CheckPermissionsInternal( true );

    private bool CheckPermissionsInternal( bool askUser )
    {
      bool access = true;
      List<string> log = new();

      // проверка права управлять уведомлениями
      if ( OperatingSystem.IsAndroidVersionAtLeast( 33 ) )
      {
        var granted = _owner.CheckSelfPermission( Android.Manifest.Permission.PostNotifications );

        _logger?.Info( _owner, $"POST_NOTIFICATIONS = {granted}" );

        if ( askUser && granted != Android.Content.PM.Permission.Granted )
        {
          if ( ++requestPermissionPostNotifyCount <= 2 )
          {
            _owner.RequestPermissions(
                new[]
                {
                  Android.Manifest.Permission.PostNotifications
                },
                100 );
          }
          else
          {
            Android.Widget.Toast.MakeText( _owner, Resource.String.setNotificationPermissionsEn, ToastLength.Long )?.Show();

            var intent = new Intent( Android.Provider.Settings.ActionApplicationDetailsSettings );
            intent.SetData( Android.Net.Uri.Parse( $"package:{_owner.PackageName}" ) );
            _owner.StartActivity( intent );
          }
        }

        if ( askUser )
          granted = _owner.CheckSelfPermission( Android.Manifest.Permission.PostNotifications );
        log.Add( $"'POST_NOTIFICATIONS' - {granted}" );

        access &= granted == Android.Content.PM.Permission.Granted;
      }

      // проверка чекания наличия сети
      {
        var granted =
            _owner.CheckSelfPermission( Android.Manifest.Permission.AccessNetworkState );

        _logger?.Info( _owner, $"ACCESS_NETWORK_STATE = {granted}" );

        if ( askUser && granted != Android.Content.PM.Permission.Granted )
        {
          if ( ++requestPermissionCheckNetworkCount <= 2 )
          {
            _owner.RequestPermissions(
                new[]
                {
                  Android.Manifest.Permission.AccessNetworkState
                },
                100 );
          }
          else
          {
            Android.Widget.Toast.MakeText( _owner, Resource.String.setNetworkCheckPermissionsEn, ToastLength.Long )?.Show();

            var intent = new Intent( Android.Provider.Settings.ActionApplicationDetailsSettings );
            intent.SetData( Android.Net.Uri.Parse( $"package:{_owner.PackageName}" ) );
            _owner.StartActivity( intent );
          }
        }

        if ( askUser )
          granted = _owner.CheckSelfPermission( Android.Manifest.Permission.AccessNetworkState );
        log.Add( $"'ACCESS_NETWORK_STATE' - {granted}" );

        access &= granted == Android.Content.PM.Permission.Granted;
      }

      if ( askUser && !access && _checkTask == null )
        _checkTask = Task.Factory.StartNew( CheckPermissionsTask, _token );

      if ( _textView != null )
        _textView.Text = string.Join( ", ", log );

      return access;
    }

    private void CheckPermissionsTask()
    {
      _logger?.Debug( this, $"CheckPermissionsTask start" );
      while ( !_token.IsCancellationRequested )
      {
        try
        {
          bool access = true;
          var tcs = new TaskCompletionSource();
          _owner.RunOnUiThread( () =>
          {
            try
            {
              access = CheckPermissionsInternal( false );
              tcs.SetResult();
            }
            catch ( Exception ex )
            {
              tcs.SetException( ex );
            }
          } );
          tcs.Task.Wait( _token );

          if ( access )
          {
            _checkTask = null;
            _logger?.Debug( this, $"CheckPermissionsTask exit" );
            break;
          }

          Task.Delay( 1000, _token ).Wait( _token );
          _logger?.Debug( this, $"CheckPermissionsTask tick" );
        }
        catch ( Exception ex )
        {
          _logger?.Error( this, $"# Error periodic check permissions: {ex.Message}|{ex.InnerException?.Message}" );
        }
      }
      _checkTask = null;
    }
  }
}
