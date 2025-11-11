
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

namespace ControlMyDevice
{
	[Activity (Label = "ChangeNameActivity")]			
	public class ChangeNameActivity : Activity
	{
		bool isBound = false;
		bool isConfigurationChange = false;
		DeviceServiceBinder binder;
		DeviceServiceConnection serviceConnection;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.ChangeName);

			EditText txtNewName = FindViewById<EditText> (Resource.Id.txtChangeNameName);
			Button btnSave = FindViewById<Button> (Resource.Id.btnChangeNameSave);
			btnSave.Click += (object sender, EventArgs e) => {
				if (isBound) {
					RunOnUiThread (() => {
						string newName = txtNewName.Text;
						binder.GetDeviceService ().ChangeName(newName);
					});
				}
			};

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

		class DeviceServiceConnection : Java.Lang.Object, IServiceConnection
		{
			ChangeNameActivity activity;
			DeviceServiceBinder binder;

			public DeviceServiceBinder Binder {
				get {
					return binder;
				}
			}

			public DeviceServiceConnection (ChangeNameActivity activity)
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

