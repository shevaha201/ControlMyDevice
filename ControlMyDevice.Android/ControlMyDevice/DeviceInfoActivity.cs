
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Net.Wifi;

namespace ControlMyDevice
{
	[Activity (Label = "DeviceInfoActivity")]			
	public class DeviceInfoActivity : BaseActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.DeviceInfo);
			WifiManager wifiManager = (WifiManager)GetSystemService (Context.WifiService);
			string macAddress = wifiManager.ConnectionInfo.MacAddress;

			TextView deviceIdTextView = FindViewById<TextView>(Resource.Id.deviceInfoDeviceId);
			deviceIdTextView.Text = macAddress;

			TextView deviceNameTextView = FindViewById<TextView>(Resource.Id.deviceInfoDeviceName);
			deviceNameTextView.Text = DeviceService.ClientName;

			Button btnChangeName = FindViewById<Button> (Resource.Id.btnChangeName);
			btnChangeName.Click += (object sender, EventArgs e) => {
				StartActivity(new Intent(this, typeof(ChangeNameActivity)));
			};

			Button bntRequestsList = FindViewById<Button> (Resource.Id.btnRequestsList);
			bntRequestsList.Click += (object sender, EventArgs e) => {
				binder.GetDeviceService ().GetDeviceRequests();
			};
		}

		public override void OnBackPressed ()
		{

		}
	}
}

