using Android.Content;
using Android.Util;
using Android.Content.PM;

namespace Passcode
{
	[BroadcastReceiver]
	public class NotificationClickReceiver : BroadcastReceiver
	{
		public override void OnReceive (Context context, Intent intent) {
			var clipboardManager = (ClipboardManager)context.GetSystemService(Context.ClipboardService);

			var message = intent.GetStringExtra ("message");
			Android.Content.ClipData clip = Android.Content.ClipData.NewPlainText(message, message);
			clipboardManager.PrimaryClip = clip;

			// Start Ingress
			Intent launchIngressIntent = context.PackageManager.GetLaunchIntentForPackage("com.nianticproject.ingress");
			context.StartActivity(launchIngressIntent);
		}
	}
}

