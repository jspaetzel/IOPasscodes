using System;
using Android.App;
using Android.Content;
using Android.Util;
using Android.Gms.Gcm;
using Android.Gms.Gcm.Iid;
using Android.Preferences;
using System.Net.Http;
using ModernHttpClient;
using Models.Passcode;
using Newtonsoft.Json;

namespace Passcode
{
	[Service(Exported = false)]
	class RegistrationIntentService : IntentService
	{
		static object locker = new object();

		public RegistrationIntentService() : base("RegistrationIntentService") { }

		protected override void OnHandleIntent (Intent intent)
		{
			try
			{
				Log.Info ("RegistrationIntentService", "Calling InstanceID.GetToken");
				lock (locker)
				{
					var instanceID = InstanceID.GetInstance (this);
					//var token = instanceID.GetToken ("426993028395", GoogleCloudMessaging.InstanceIdScope, null); // DEBUG
					var token = instanceID.GetToken ("912302796641", GoogleCloudMessaging.InstanceIdScope, null); // RELEASE

					Log.Info ("RegistrationIntentService", "GCM Registration Token: " + token);
					SendRegistrationToAppServer (token);
					Subscribe (token);
				}
			}
			catch (Exception e)
			{
                Log.Debug("RegistrationIntentService", "Failed to get a registration token. " + e.Message);
				return;
			}
		}

		void SendRegistrationToAppServer (string token)
		{
			SetAgentGCMToken (token);
			//Log.Debug ("SendRegistrationToAppServer", token);
			// Add custom implementation here as needed.
		}

		async void SetAgentGCMToken(string token)
		{
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this); 
			var agentId = prefs.GetString ("agentId", null);
			var agentToken = prefs.GetString ("token", null);

			var httpClient = new HttpClient(new NativeMessageHandler());
			string url = "http://enl.io/api/gcm/passcode/" + token + "?token=" + agentToken;


			using (HttpClient client =  new HttpClient(new NativeMessageHandler()))
			using (HttpResponseMessage response = await client.GetAsync(url))
			{
				HttpContent content = response.Content;
				string result = await content.ReadAsStringAsync();
				Log.Debug ("SetAgentGCMToken", result);
			}
		}

		void Subscribe (string token)
		{
			var pubSub = GcmPubSub.GetInstance(this);
			pubSub.Subscribe(token, "/topics/passcodes", null);
		}
	}
}