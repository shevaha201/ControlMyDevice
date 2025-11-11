using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Widget;

using Java.IO;

using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace ControlMyDevice
{
	public static class Info {
		public static File File;
		public static File Dir;     
		public static string RequestUserId;
	}

	[Activity (Label = "TakePhotoActivity", ScreenOrientation = ScreenOrientation.Portrait)]			
	public class TakePhotoActivity : BaseActivity
	{
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			Uri contentUri = Uri.FromFile(Info.File);
			mediaScanIntent.SetData(contentUri);
			SendBroadcast(mediaScanIntent);
			string base64String = FileToBase64String(Info.File);
			if (!string.IsNullOrEmpty(base64String)) {
				binder.GetDeviceService ().ResponseTakePhotoRequest (Info.RequestUserId, base64String);
			}

			Finish ();
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.RecordVideo);

			if (this.Intent.Extras != null) {
				Info.RequestUserId = this.Intent.Extras.GetString ("RequestUserId");
			}

			if (IsThereAnAppToTakePictures())
			{
				CreateDirectoryForPictures();

				Button btnOpenCamera = FindViewById<Button>(Resource.Id.btnOpenCamera);
				btnOpenCamera.Click += TakePicture;

				btnOpenCamera.PerformClick ();
			}
		}

		private bool IsThereAnAppToTakePictures()
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities = PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		private void TakePicture(object sender, EventArgs eventArgs)
		{
			Intent intent = new Intent(MediaStore.ActionImageCapture);

			Info.File = new File(Info.Dir, String.Format("myPhoto_{0}.jpg", Guid.NewGuid()));

			intent.PutExtra(MediaStore.ExtraOutput, Uri.FromFile(Info.File));

			StartActivityForResult(intent, 0);
		}

		private void CreateDirectoryForPictures()
		{
			Info.Dir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "ControlMyDevice");
			if (!Info.Dir.Exists())
			{
				Info.Dir.Mkdirs();
			}
		}

		private string FileToBase64String(Java.IO.File file){
			string str = string.Empty;

			if (file.Exists()) {
				byte[] bytes = null;
				int fileLength = (int)(file.Length());
				bytes = new byte[fileLength];

				using (FileInputStream fileInputStream = new FileInputStream (file)) {
					fileInputStream.Read (bytes);
				}
				str = Convert.ToBase64String (bytes);
			}

			return str;
		}
	}
}

