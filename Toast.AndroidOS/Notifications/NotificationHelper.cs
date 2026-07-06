using System.Runtime.Versioning;

using Android.App;
using Android.Content;
using Android.OS;

using AndroidX.Core.App;

using Toast.Core.Models;

namespace Toast.AndroidOS.Notifications;

internal static class NotificationHelper
{
  public const string ChannelId = "Toast.Agent";
  public const int NotificationId = 1;

  public static void EnsureChannel( Context context )
  {
    if ( OperatingSystem.IsAndroidVersionAtLeast( 26 ) )
      EnsureChannel26( context );
  }

  public static Notification CreateNotification(
      Context context,
      string title,
      string text )
  {
    var builder = new NotificationCompat.Builder( context, ChannelId );

    if ( builder == null )
      return new Notification( Resource.Drawable.shuttle, "### Не удалось создать сообщение" );

    builder
        ?.SetContentTitle( title )
        ?.SetContentText( text )
        ?.SetSmallIcon( Resource.Drawable.shuttle )
        ?.SetOngoing( true )
        ?.SetOnlyAlertOnce( true )
        ?.SetSilent( true )
        ?.SetPriority( ( int ) NotificationPriority.Low );

    return builder?.Build() ?? new Notification( Resource.Drawable.shuttle, "### Не удалось создать сообщение" );
  }

  public static Notification CreateNotification(
    Context context,
    AgentState state ) => CreateNotification( context, GetTitle(), GetText( state ) );

  public static Notification CreateNotification(
    Context context,
    AgentStatus status ) => CreateNotification( context, GetTitle(), GetText( status ) );

  public static void UpdateNotification(
    Context context,
    AgentStatus status )
  {
    NotificationManagerCompat
        .From( context )
        ?.Notify(
            NotificationId,
            CreateNotification( context, status ) );
  }

  private static string GetText( AgentState state )
  {
    return state switch
    {
      AgentState.Starting => "Starting...",
      AgentState.Waiting => "Waiting for next poll",
      AgentState.Polling => "Polling server...",
      AgentState.Executing => "Executing commands...",
      AgentState.Offline => "Server unavailable",
      AgentState.Error => "Execution error",
      AgentState.Stopping => "Stopping...",
      AgentState.Stopped => "Stopped",
      _ => state.ToString()
    };
  }

  private static string GetText( AgentStatus status ) => GetText( status.State ) + ( string.IsNullOrWhiteSpace( status.Details ) ? string.Empty : $": ({status.Details})" );

  private static string GetTitle() => "Toast Agent";

  [SupportedOSPlatform( "android26.0" )]
  private static void EnsureChannel26( Context context )
  {
    var manager = ( NotificationManager? ) context.GetSystemService( Context.NotificationService );

    if ( manager == null )
      return;

    if ( manager.GetNotificationChannel( ChannelId ) != null )
      return;

    var channel = new NotificationChannel(
        ChannelId,
        "Toast Agent",
        NotificationImportance.Low );

    channel.Description = "Background service";

    manager.CreateNotificationChannel( channel );
  }

}
