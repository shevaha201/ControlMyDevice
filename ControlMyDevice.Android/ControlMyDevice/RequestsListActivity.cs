using System.Linq;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;

namespace ControlMyDevice
{
	[Activity (Label = "RequestsListActivity")]			
	public class RequestsListActivity : BaseActivity
	{
		private ListView _requestList;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.RequestsList);
			if (this.Intent.Extras != null) {
				string message = this.Intent.Extras.GetString ("Message");
				RunOnUiThread (() => {
					Toast.MakeText (this, message, ToastLength.Short).Show();
				});
			}
			_requestList = FindViewById<ListView> (Resource.Id.lsRequestList);
			_requestList.Adapter = new RequestsAdapter (this, DeviceService.Requests, AcceptClick, RejectClick);
		}

		protected int AcceptClick(int requestId){
			binder.GetDeviceService ().AcceptRequest(requestId, this);
			RequestItem requestItem = DeviceService.Requests.Where (t => t.DeviceUserRequestId == requestId).First ();
			DeviceService.Requests.Remove (requestItem);
			RequestsAdapter adapter = _requestList.Adapter as RequestsAdapter;
			adapter.NotifyDataSetChanged ();
			return 0;
		}

		protected int RejectClick(int requestId){
			binder.GetDeviceService ().RejectRequest(requestId, this);
			RequestItem requestItem = DeviceService.Requests.Where (t => t.DeviceUserRequestId == requestId).First ();
			DeviceService.Requests.Remove (requestItem);
			RequestsAdapter adapter = _requestList.Adapter as RequestsAdapter;
			adapter.NotifyDataSetChanged ();
			return 0;
		}

		public override void OnBackPressed ()
		{
			StartActivity (new Intent(this, typeof(DeviceInfoActivity)));
		}
	}
}

