using System;
using Android.App;
using Android.OS;
using Android.Widget;

namespace ControlMyDevice
{
	[Activity (Label = "ChangeNameActivity")]			
	public class ChangeNameActivity : BaseActivity
	{
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
		}
	}
}

