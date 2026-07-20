using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;

using Toast.Core.Utilities;

namespace Toast.AndroidOS.Activities
{
  public static class ActivityHelper
  {

    public static void SetTextFor( this Activity owner, TextView? textView, string text, string? delimiterLine = null )
    {
      if ( textView != null )
        owner.RunOnUiThread( () =>
          textView.Text = text
         );
    }


    public static void AppendLine( this Activity owner, TextView? textView, string text, string? delimiterLine = null, int maxLength = 10000 )
    {
      if ( textView != null )
        owner.RunOnUiThread( () =>
        {
          textView.Text =
          (
              textView.Text
            + System.Environment.NewLine
            + ( string.IsNullOrEmpty( delimiterLine ) ? String.Empty : ( delimiterLine + System.Environment.NewLine ) )
            + text
          ).TrimLeft( maxLength );
        } );
    }

    public static void PrependLine( this Activity owner, TextView? textView, string text, string? delimiterLine = null, int maxLength = 10000 )
    {
      if ( textView != null )
        owner.RunOnUiThread( () =>
        {
          textView.Text =
          (
              text
            + System.Environment.NewLine
            + ( string.IsNullOrEmpty( delimiterLine ) ? String.Empty : ( delimiterLine + System.Environment.NewLine ) )
            + textView.Text
          ).TrimRight( maxLength );
        } );
    }

  }
}
