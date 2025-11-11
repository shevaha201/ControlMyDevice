using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace ControlMyDevice
{

	[Activity (Label = "RecordVideoActivity", ScreenOrientation = ScreenOrientation.Portrait)]			
	public class RecordVideoActivity : BaseActivity
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
				binder.GetDeviceService ().ResponseRecordVideoRequest (Info.RequestUserId, base64String);
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
				btnOpenCamera.Click += RecordVideo;

				btnOpenCamera.PerformClick();
			}
		}

		private bool IsThereAnAppToTakePictures()
		{
			Intent intent = new Intent(MediaStore.ActionVideoCapture);
			IList<ResolveInfo> availableActivities = PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
		}

		private void RecordVideo(object sender, EventArgs eventArgs)
		{
			Intent intent = new Intent(MediaStore.ActionVideoCapture);
			Info.File = new File(Info.Dir, String.Format("myVideo_{0}.mp4", Guid.NewGuid()));

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

//		protected override void OnCreate (Bundle bundle)
//		{
//			base.OnCreate (bundle);
//			SetContentView(Resource.Layout.RecordVideo);
//
//			if (this.Intent.Extras != null) {
//				Info.RequestUserId = this.Intent.Extras.GetString ("RequestUserId");
//			}
//
//			if (IsThereAnAppToTakePictures())
//			{
//				CreateDirectoryForPictures();
//
//				Button button = FindViewById<Button>(Resource.Id.btnOpenCamera);
//				button.Click += TakePicture;
//			}
//		}
//
//		private MediaRecorder _recorder;
//
//		protected override void OnCreate (Bundle bundle)
//		{
//			base.OnCreate (bundle);
//
//			SetContentView (Resource.Layout.RecordVideo);
//
//			string videoFilePath = GetVideoFilePath ();
//
//			Button btnRecord = FindViewById<Button> (Resource.Id.btnRecord);
//			MediaStore.Actio
//			Button btnStop = FindViewById<Button> (Resource.Id.btnStop);
//			VideoView videoView = FindViewById<VideoView> (Resource.Id.videoView);
//
//			btnStop.Enabled = false;
//
//			btnRecord.Click += delegate {
//				videoView.StopPlayback ();
//
//				_recorder = new MediaRecorder ();
//				_recorder.SetVideoSource (VideoSource.Camera); 
//				_recorder.SetAudioSource (AudioSource.Mic);              
//				_recorder.SetOutputFormat (OutputFormat.Default);
//				_recorder.SetVideoEncoder (VideoEncoder.Default); 
//				_recorder.SetAudioEncoder (AudioEncoder.Default);      
//				_recorder.SetOutputFile (videoFilePath);
//				_recorder.SetPreviewDisplay (videoView.Holder.Surface);
//				_recorder.SetOrientationHint(90);
//				_recorder.Prepare ();
//				_recorder.Start ();
//
//				btnRecord.Enabled = false;
//				btnStop.Enabled = true;
//			};
//
//			btnStop.Click += delegate {
//				if (_recorder != null) {
//					_recorder.Stop ();
//					_recorder.Release ();
//				}
//				btnRecord.Enabled = true;
//				btnStop.Enabled = false;
//			};
//		}
//
//		private string GetVideoFilePath()
//		{
//			Info.Dir = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), "ControlMyDevice");
//			if (!Info.Dir.Exists())
//			{
//				Info.Dir.Mkdirs();
//			}
//
//			string filePath = Environment.GetExternalStoragePublicDirectory (Environment.DirectoryPictures) + String.Format ("/ControlMyDevice/video_{0}.mp4", Guid.NewGuid ());
//			return filePath;
//		}
//
//		protected override void OnDestroy ()
//		{
//			base.OnDestroy ();
//
//			if (_recorder != null) {
//				_recorder.Release ();
//				_recorder.Dispose ();
//				_recorder = null;
//			}
//		}
	}
}

