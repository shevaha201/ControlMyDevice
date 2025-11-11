using System;
using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Newtonsoft.Json.Linq;
using Android.Provider;
using WebSocket4Net;
using Xamarin.Contacts;
using Xamarin.Geolocation;

namespace ControlMyDevice
{

	[Service]
	public class DeviceService : Service
	{
		private static string WebSocketServerUrl = "wss://controlmydevice.herokuapp.com/";
		public static string _clientId = null;
		public static string ClientName = null;
		public static List<RequestItem> Requests = new List<RequestItem>();
		private static WebSocket _webSocket; 
		private string _identifier;
		private static Context _currentContext;

		DeviceServiceBinder binder;

		public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
		{
			WifiManager wifiManager = (WifiManager)GetSystemService (Context.WifiService);
			_identifier = wifiManager.ConnectionInfo.MacAddress;

			Process ();
			return StartCommandResult.Sticky;
		}

		public override Android.OS.IBinder OnBind (Android.Content.Intent intent)
		{
			binder = new DeviceServiceBinder (this);
			return binder;
		}

		public void Process() {
			InitWebSocket();
		}

//		public void ShowNotification(string message){
//			Notification.Builder builder = new Notification.Builder (this)
//				.SetContentTitle ("ControlMyDevice")
//				.SetContentText (message)
//				.SetSmallIcon (Resource.Drawable.Icon);
//			Notification notification = builder.Build ();
//			NotificationManager notificationManager =
//				GetSystemService (Context.NotificationService) as NotificationManager;
//			notificationManager.Notify (0, notification);
//		}

		public void InitWebSocket(){
			_webSocket = new WebSocket (WebSocketServerUrl);
			_webSocket.AllowUnstrustedCertificate = true;
			_webSocket.Opened += HandleOpened;
			_webSocket.Error += HandleError;
			_webSocket.Closed += HandleClosed;
			_webSocket.MessageReceived += HandleMessageReceived;
			_webSocket.Open ();
		}

		void HandleOpened (object sender, EventArgs e) {
			Console.WriteLine ("WebSocket opened");
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			_webSocket.Send(messageProcessor.CreateConnectRequestMessage());
		}

		void HandleError (object sender, SuperSocket.ClientEngine.ErrorEventArgs e) {
			Console.WriteLine (e.ToString());
		}

		void HandleClosed (object sender, EventArgs e) {
			Console.WriteLine("closed");
			var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService (Activity.ConnectivityService);
			while (connectivityManager.ActiveNetworkInfo == null || !connectivityManager.ActiveNetworkInfo.IsConnected){
				System.Threading.Thread.Sleep(5000);
				Console.WriteLine ("No internet connection");
			}
			if (_webSocket.State != WebSocketState.Open && _webSocket.State != WebSocketState.Connecting)
				_webSocket.Open();
		}

		void HandleMessageReceived (object sender, MessageReceivedEventArgs e) {
			JObject jsonObject = JObject.Parse (e.Message);
			string commandName = (string)jsonObject [JsonStructure.Command.NodeName][JsonStructure.Command.Name];
			switch (commandName) {
			case (Command.Connect.Response):
				ProcessConnectResponse (jsonObject);
				break;
			case (Command.ClientInfo.Response):
				ProcessDeviceInfoResponse (jsonObject);
				break;
			case (Command.ChangeDeviceName.Response):
				ProcessChangeDeviceNameResponse (jsonObject);
				break;
			case (Command.DeviceRequestList.Response):
				ProcessDeviceRequestsListResponse (jsonObject);
				break;
			case (Command.DeviceAcceptRequest.Response):
				ProcessAcceptResponse(jsonObject);
				break;
			case (Command.DeviceRejectRequest.Response):
				ProcessRejectResponse (jsonObject);
				break;
			case (Command.Device.GetContacts.Request):
				ProcessGetContactsRequest (jsonObject);
				break;
			case (Command.Device.GetCallHistory.Request):
				ProcessGetCallHistoryRequest (jsonObject);
				break;
			case (Command.Device.GetLocation.Request):
				ProcessGetLocationRequest (jsonObject);
				break;
			case (Command.Device.TakePhoto.Request):
				ProcessTakePhotoRequest (jsonObject);
				break;
			case (Command.Device.RecordVideo.Request):
				ProcessRecordVideoRequest (jsonObject);
				break;
			default:
				break;
			}
		}

		void ProcessConnectResponse(JObject json){
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			string status = messageProcessor.ProcessConnectResponse(json);
			if (status == Common.ResponseStatus.Success) {
				_webSocket.Send (messageProcessor.CreateClientInfoRequestMessage ());
			}
		}

		void ProcessDeviceInfoResponse(JObject json){
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			string name = messageProcessor.ProcessDeviceInfoResponse(json);
			ClientName = name;
			Intent deviceInfoIntent = new Intent(this, typeof(DeviceInfoActivity));
			deviceInfoIntent.SetFlags(ActivityFlags.NewTask);
			StartActivity (deviceInfoIntent);
		}

		public void ChangeName(string newName){
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			_webSocket.Send (messageProcessor.CreateChangeDeviceNameRequestMessage (newName));
		}

		void ProcessChangeDeviceNameResponse(JObject json){
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			string name = messageProcessor.ProcessChangeDeviceNameResponse(json);
			ClientName = name;
			Intent deviceInfoIntent = new Intent(this, typeof(DeviceInfoActivity));
			deviceInfoIntent.SetFlags(ActivityFlags.NewTask);
			StartActivity (deviceInfoIntent);
		}

		public void GetDeviceRequests(){
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			_webSocket.Send (messageProcessor.CreateDeviceRequestsListRequestMessage());
		}

		void ProcessDeviceRequestsListResponse(JObject json){
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			Requests = messageProcessor.ProcessDeviceRequestsListResponse(json);
			Intent deviceRequestsIntent = new Intent(this, typeof(RequestsListActivity));
			deviceRequestsIntent.SetFlags(ActivityFlags.NewTask);
			StartActivity (deviceRequestsIntent);
		}

		public void AcceptRequest(int requestId, Context context){
			_currentContext = context;
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			_webSocket.Send (messageProcessor.CreateDeviceAcceptRequestMessage(requestId));
		}

		public void RejectRequest(int requestId, Context context){
			_currentContext = context;
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			_webSocket.Send (messageProcessor.CreateDeviceRejectRequestMessage(requestId));
		}

		public void ProcessAcceptResponse(JObject json) {
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			string status = messageProcessor.ProcessDeviceAcceptRequestResponse (json);
			string message = string.Empty;
			if (status == Common.ResponseStatus.Success) {
				message = "Request accepted";
			} else {
				message = "Forbidden";
			}
			Intent deviceRequestsIntent = new Intent(this, typeof(RequestsListActivity));
			deviceRequestsIntent.SetFlags(ActivityFlags.NewTask);
			Bundle bundle = new Bundle ();
			bundle.PutString("Message", message);
			deviceRequestsIntent.PutExtras (bundle);
			StartActivity (deviceRequestsIntent);
		}

		public void ProcessRejectResponse(JObject json){
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			string status = messageProcessor.ProcessDeviceAcceptRequestResponse (json);
			string message = string.Empty;
			if (status == Common.ResponseStatus.Success) {
				message = "Request rejected";
			} else {
				message = "Forbidden";
			}
			Intent deviceRequestsIntent = new Intent(this, typeof(RequestsListActivity));
			deviceRequestsIntent.SetFlags(ActivityFlags.NewTask);
			Bundle bundle = new Bundle ();
			bundle.PutString("Message", message);
			deviceRequestsIntent.PutExtras (bundle);
			StartActivity (deviceRequestsIntent);
		}

		public void ProcessGetContactsRequest(JObject json) {
			string requestUserId = (string)json["command_parameters"]["request_user_id"];
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			string message = messageProcessor.CreateGetContactsResponseMessage (
				requestUserId,
				GetContacts ()
					.Select(t => new ContactItem {
						Name = t.Value,
						Number = t.Key
					})
					.ToList ());
			_webSocket.Send (message);
		}

		public void ProcessGetCallHistoryRequest(JObject json) {
			string requestUserId = (string)json["command_parameters"]["request_user_id"];
			var messageProcessor = new MessageProcessor(_identifier, _clientId);
			string message = messageProcessor.CreateGetCallHistoryResponseMessage (
				requestUserId,
				GetCallHistory ()
			);
			_webSocket.Send (message);
		}

		public void ProcessGetLocationRequest(JObject json) {
			string requestUserId = (string)json["command_parameters"]["request_user_id"];
			var geolocator = new Geolocator (this);
			geolocator.GetPositionAsync (10000).ContinueWith (t => {
				var messageProcessor = new MessageProcessor(_identifier, _clientId);
				string message = messageProcessor.CreateGetLocationResponseMessage (requestUserId, t.Result.Latitude.ToString(), t.Result.Longitude.ToString());
				_webSocket.Send (message);
			});
		}

		public void ProcessTakePhotoRequest(JObject json) {
			string requestUserId = (string)json["command_parameters"]["request_user_id"];
			Intent takePhotoActivityIntent = new Intent (this, typeof(TakePhotoActivity));
			Bundle bundle = new Bundle ();
			bundle.PutString("RequestUserId", requestUserId);
			takePhotoActivityIntent.PutExtras (bundle);
			takePhotoActivityIntent.SetFlags(ActivityFlags.NewTask);
			StartActivity (takePhotoActivityIntent);
		}

		public void ResponseTakePhotoRequest(string requestUserId, string base64StringPhoto) {
			var messageProcessor = new MessageProcessor (_identifier, _clientId);
			_webSocket.Send(messageProcessor.CreateTakePhotoResponse(requestUserId, base64StringPhoto));
		}

		public void ProcessRecordVideoRequest(JObject json) {
			string requestUserId = (string)json["command_parameters"]["request_user_id"];
			Intent recordVideoActivityIntent = new Intent (this, typeof(RecordVideoActivity));
			Bundle bundle = new Bundle ();
			bundle.PutString("RequestUserId", requestUserId);
			recordVideoActivityIntent.PutExtras (bundle);
			recordVideoActivityIntent.SetFlags(ActivityFlags.NewTask);
			StartActivity (recordVideoActivityIntent);

		}

		public void ResponseRecordVideoRequest(string requestUserId, string base64StringVideo) {
			var messageProcessor = new MessageProcessor (_identifier, _clientId);
			_webSocket.Send(messageProcessor.CreateRecordVideoResponse(requestUserId, base64StringVideo));
		}


		private IDictionary<string, string> GetContacts() {
			var addressBook = new AddressBook (this);
			IDictionary<string, string> contacts = new Dictionary<string, string>();
			foreach (var contact in addressBook) {
				if (contact.Phones != null && contact.Phones.Count () > 0) {
					foreach (var phone in contact.Phones) {
						string number = phone.Number.TrimStart (Common.TrimFromMobileNumber.ToCharArray ()).Replace(" ", "");
						if (!contacts.ContainsKey(number))
							contacts.Add (number, contact.DisplayName);
					}
				}
			}

			return contacts;
		}

		private ICollection<CallHistoryItem> GetCallHistory() {
			string[] columns = {
				CallLog.Calls.Number,
				CallLog.Calls.Type,
				CallLog.Calls.Date
			};

			var cursor = this.ContentResolver.Query (Android.Net.Uri.Parse ("content://call_log/calls"), columns, null, null, CallLog.Calls.DefaultSortOrder);

			ICollection<CallHistoryItem> callHistoryItems = new List<CallHistoryItem> ();
			IDictionary<string, string> contacts = GetContacts ();
			var baseDateTime = new DateTime (1970, 1, 1).Add(DateTimeOffset.Now.Offset);

			while (cursor.MoveToNext ()) {
				var callHistoryItem = new CallHistoryItem {
					Number = cursor.GetString (cursor.GetColumnIndex (CallLog.Calls.Number)).TrimStart(Common.TrimFromMobileNumber.ToCharArray()).Replace(" ", ""),
					DateTime = (baseDateTime.AddMilliseconds(cursor.GetLong(cursor.GetColumnIndex (CallLog.Calls.Date)))).ToString("yyyy.MM.dd HH:mm"),
					Type = Common.CallTypes[cursor.GetString (cursor.GetColumnIndex (CallLog.Calls.Type))]
				};
				if (contacts.ContainsKey (callHistoryItem.Number))
					callHistoryItem.Name = contacts [callHistoryItem.Number];

				callHistoryItems.Add (callHistoryItem);
			}

			return callHistoryItems;
		}
	}

	public class DeviceServiceBinder : Binder
	{
		DeviceService service;

		public DeviceServiceBinder (DeviceService service)
		{
			this.service = service;
		}

		public DeviceService GetDeviceService ()
		{
			return service;
		}
	}
}

