using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Android.Gms.Common;
using Android.Util;
using Android.Preferences;
using System.Net;
using System.Net.Http;
using ModernHttpClient;
using Models.Passcode;
using Newtonsoft.Json;
using System.Collections.Generic;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Passcode
{
	[Activity (Label = "Passcodes", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, ListView.IOnItemClickListener
	{
		private string agentId;
        private List<Code> codes;
        private List<String> codesString;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

            AppCenter.Start("2169b3d5-902d-4109-a3a9-22f08c49b40d", typeof(Analytics), typeof(Crashes));

			SetContentView (Resource.Layout.Main);

			LinearLayout pendingKeyLayout = FindViewById<LinearLayout> (Resource.Id.pendingKeyLayout);
			LinearLayout activeUserLayout = FindViewById<LinearLayout> (Resource.Id.activeUserLayout);

			Button button = FindViewById<Button> (Resource.Id.keyUpdateSubmitButton);

			updateView ();
            GetLatestPasscodes();

			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this); 
			button.Click += delegate {
				var keyInputBox = FindViewById<EditText> (Resource.Id.keyInputBox).Text;
				GetUserData(keyInputBox);
			};
		}

		private void updateView() {
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this); 
			LinearLayout pendingKeyLayout = FindViewById<LinearLayout> (Resource.Id.pendingKeyLayout);
			LinearLayout activeUserLayout = FindViewById<LinearLayout> (Resource.Id.activeUserLayout);

			this.agentId = prefs.GetString ("agentId", null);

			if (agentId == null) {
				pendingKeyLayout.Visibility = ViewStates.Visible;
				activeUserLayout.Visibility = ViewStates.Gone;
			} else {
				pendingKeyLayout.Visibility = ViewStates.Gone;
				activeUserLayout.Visibility = ViewStates.Visible;
			}
		}

		async void GetUserData(string token)
		{
			ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this); 
			ISharedPreferencesEditor editor = prefs.Edit();
			editor.PutString("token", token).Apply ();

			var httpClient = new HttpClient(new NativeMessageHandler());
			string url = "https://enl.io/api/whoami?token=" + token;
			using (HttpClient client =  new HttpClient(new NativeMessageHandler()))
			using (HttpResponseMessage response = await client.GetAsync(url))
			{
				HttpContent content = response.Content;
				string result = await content.ReadAsStringAsync();
				Whois agent = JsonConvert.DeserializeObject<Whois> (result);
				if (agent != null) {
					editor.PutString ("agentId", agent.agent_id).Apply ();

					TextView helloText = FindViewById<TextView> (Resource.Id.helloText);
					helloText.Text = "Hello " + agent.agent_name + ",";

					updateView();
                    GetLatestPasscodes();

					// Setup Update GCM settings
					if (IsPlayServicesAvailable ()) {
						var intent = new Intent (this, typeof(RegistrationIntentService));
						StartService (intent);
					}
				}
			}
		}

        async void GetLatestPasscodes()
        {
            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            var token = prefs.GetString("token", "");

            if (!String.IsNullOrEmpty(token)){
                var httpClient = new HttpClient(new NativeMessageHandler());
                string url = "https://enl.io/api/passcodes?token=" + token;
                using (HttpClient client = new HttpClient(new NativeMessageHandler()))
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    HttpContent content = response.Content;
                    string result = await content.ReadAsStringAsync();
                    codes = JsonConvert.DeserializeObject<List<Code>>(result);
                    if (codes.Count > 0)
                    {
                        ListView listView = FindViewById<ListView>(Resource.Id.passcodesList);
                        codesString = new List<string>();
                        foreach (var code in codes)
                        {
                            var codeString = code.code;
                            codesString.Add(codeString);
                        }
                        var listAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, codesString);
                        listView.Adapter = listAdapter;
                        listView.OnItemClickListener = this;
                    }
                }
            }
        }

        public void OnItemClick(AdapterView parent, View view, int position, long id)
        {
            var t = codesString[position];

            var clipboardManager = (ClipboardManager)this.GetSystemService(Context.ClipboardService);

            var message = "Copied " + t + " to clipboard";
            Android.Content.ClipData clip = Android.Content.ClipData.NewPlainText(t, t);
            clipboardManager.PrimaryClip = clip;

            Toast.MakeText(this, message, ToastLength.Short).Show();
        }

		public bool IsPlayServicesAvailable ()
		{
			
			int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
			if (resultCode != ConnectionResult.Success)
			{
				if (GoogleApiAvailability.Instance.IsUserResolvableError (resultCode)) {
					var error = GoogleApiAvailability.Instance.GetErrorString (resultCode);
					Log.Debug ("IsPlayServicesAvailable", error);
				} else {
					Log.Debug("IsPlayServicesAvailable", "Sorry, this device is not supported");
					Finish ();
				}
				return false;
			}
			else
			{
				Log.Debug("IsPlayServicesAvailable", "Google Play Services are available.");
				return true;
			}
		}
	}
}


