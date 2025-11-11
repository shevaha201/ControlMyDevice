using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;

namespace ControlMyDevice
{
	[Activity (Label = "ControlMyDevice", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			WifiManager wifiManager = (WifiManager)GetSystemService (Context.WifiService);
			string macAddress = wifiManager.ConnectionInfo.MacAddress;

			Intent serviceIntent = new Intent (ApplicationContext, typeof(DeviceService));
			serviceIntent.SetFlags (ActivityFlags.NewTask);
			StartService(serviceIntent);
		}
	}
}


