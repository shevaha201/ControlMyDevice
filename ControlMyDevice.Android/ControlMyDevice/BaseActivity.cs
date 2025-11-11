using Android.App;
using Android.Net.Wifi;
using Android.Content;
using Android.OS;

namespace ControlMyDevice
{
	public class BaseActivity : Activity
	{
		protected bool isBound = false;
		protected bool isConfigurationChange = false;
		protected DeviceServiceBinder binder;
		protected DeviceServiceConnection serviceConnection;

		protected string GetIdentifier(){
			WifiManager wifiManager = (WifiManager)GetSystemService (Context.WifiService);
			return wifiManager.ConnectionInfo.MacAddress;
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Create your application here
		}

		protected override void OnStart ()
		{
			base.OnStart ();

			var deviceServiceIntent = new Intent (this, typeof(DeviceService));
			serviceConnection = new DeviceServiceConnection (this);
			BindService (deviceServiceIntent, serviceConnection, Bind.AutoCreate);
		}

		public override Java.Lang.Object OnRetainNonConfigurationInstance ()
		{
			base.OnRetainNonConfigurationInstance ();

			isConfigurationChange = true;

			return serviceConnection;
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();

			if (!isConfigurationChange) {
				if (isBound) {
					UnbindService (serviceConnection);
					isBound = false;
				}
			}
		}

		protected class DeviceServiceConnection : Java.Lang.Object, IServiceConnection
		{
			BaseActivity activity;
			DeviceServiceBinder binder;

			public DeviceServiceBinder Binder {
				get {
					return binder;
				}
			}

			public DeviceServiceConnection (BaseActivity activity)
			{
				this.activity = activity;
			}

			public void OnServiceConnected (ComponentName name, IBinder service)
			{
				var deviceServiceBinder = service as DeviceServiceBinder;

				if (deviceServiceBinder != null) {
					activity.binder = deviceServiceBinder;
					activity.isBound = true;

					// keep instance for preservation across configuration changes
					this.binder = deviceServiceBinder;
				}
			}

			public void OnServiceDisconnected (ComponentName name)
			{
				activity.isBound = false;
			}
		}
	}
}

