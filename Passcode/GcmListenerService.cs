using Android.App;
using Android.Content;
using Android.OS;
using Android.Gms.Gcm;
using Android.Util;
using Android.Support.V4.App;

namespace Passcode
{
	[Service (Exported = false), IntentFilter (new [] { "com.google.android.c2dm.intent.RECEIVE" })]
	public class MyGcmListenerService : GcmListenerService
	{
		public override void OnMessageReceived (string from, Bundle data)
		{
			var message = data.GetString ("message");
			Log.Debug ("MyGcmListenerService", "From:    " + from);
			Log.Debug ("MyGcmListenerService", "Message: " + message);
			SendNotification (message);
		}

		void SendNotification (string message)
		{
			var intent = new Intent (this, typeof(NotificationClickReceiver));
			intent.PutExtra ("message", message);

			intent.AddFlags (ActivityFlags.ClearTop);

			var pendingIntent = PendingIntent.GetBroadcast (this, 0, intent, PendingIntentFlags.OneShot);

			var notificationBuilder = new NotificationCompat.Builder(this)
				.SetSmallIcon (Resource.Drawable.ic_notification)
				.SetContentTitle ("New Passcode")
				.SetContentText (message)
				.SetAutoCancel (true)
				.SetContentIntent (pendingIntent)
				.SetGroup("PASSCODES");

			var notificationManager = GetSystemService (Context.NotificationService) as NotificationManager;
			notificationManager.Notify (0, notificationBuilder.Build());
		}
	}
}