using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;

namespace ControlMyDevice
{
	public class MessageProcessor
	{
		private static string ClientType = "android";
		private string _identifier;
		private string _clientId;

		public MessageProcessor(string identifier, string clientId){
			_identifier = identifier;
			_clientId = clientId;
		}

		public JObject CreateBaseRequestMessage(string commandName){
			JObject json = new JObject (
				new JProperty("command",
					new JObject(
						new JProperty("name", commandName)
					)
				),
				new JProperty("client_info",
					new JObject(
						new JProperty("id", _clientId),
						new JProperty("type", ClientType)
					)
				),
				new JProperty("identifier", _identifier)
			);
			return json;
		}

		public string CreateConnectRequestMessage (){
			JObject json = CreateBaseRequestMessage (Command.Connect.Request);
			return json.ToString();		
		}

		public string CreateClientInfoRequestMessage (){
			JObject json = CreateBaseRequestMessage (Command.ClientInfo.Request);
			return json.ToString();		
		}

		public string CreateChangeDeviceNameRequestMessage (string newName){
			JObject json = CreateBaseRequestMessage (Command.ChangeDeviceName.Request);
			json.Add (
				new JProperty ("command_parameters",
					new JObject(
						new JProperty("new_name", newName)
					)
			));
			return json.ToString();		
		}

		public string CreateDeviceRequestsListRequestMessage (){
			JObject json = CreateBaseRequestMessage (Command.DeviceRequestList.Request);
			return json.ToString();		
		}

		public string CreateDeviceAcceptRequestMessage (int requestId){
			JObject json = CreateBaseRequestMessage (Command.DeviceAcceptRequest.Request);
			json.Add (
				new JProperty ("command_parameters",
					new JObject(
						new JProperty("request_id", requestId)
					)
				)
			);
			return json.ToString();		
		}

		public string CreateDeviceRejectRequestMessage (int requestId){
			JObject json = CreateBaseRequestMessage (Command.DeviceRejectRequest.Request);
			json.Add (
				new JProperty ("command_parameters",
					new JObject(
						new JProperty("request_id", requestId)
					)
				)
			);
			return json.ToString();		
		}

		public string CreateGetContactsResponseMessage(string requestUserId, ICollection<ContactItem> contactItems){
			JObject json = CreateBaseRequestMessage (Command.Device.GetContacts.Response);
			json.Add (
				new JProperty ("command_parameters",
					new JObject (
						new JProperty ("request_user_id", requestUserId),
						new JProperty ("contacts", JsonConvert.SerializeObject(contactItems))
					)
				)
			);
			return json.ToString ();
		}

		public string CreateGetCallHistoryResponseMessage(string requestUserId, ICollection<CallHistoryItem> callHistoryItems){
			JObject json = CreateBaseRequestMessage (Command.Device.GetCallHistory.Response);
			json.Add (
				new JProperty ("command_parameters",
					new JObject (
						new JProperty ("request_user_id", requestUserId),
						new JProperty ("call_history_items", JsonConvert.SerializeObject(callHistoryItems))
					)
				)
			);
			return json.ToString ();
		}

		public string CreateGetLocationResponseMessage(string requestUserId, string latitude, string longitude){
			JObject json = CreateBaseRequestMessage (Command.Device.GetLocation.Response);
			json.Add (
				new JProperty ("command_parameters",
					new JObject (
						new JProperty ("request_user_id", requestUserId),
						new JProperty ("location_latitude", latitude),
						new JProperty ("location_longitude", longitude)
					)
				)
			);
			return json.ToString ();
		}

		public string CreateTakePhotoResponse(string requestUserId, string base64StringPhoto){
			JObject json = CreateBaseRequestMessage (Command.Device.TakePhoto.Response);
			json.Add (
				new JProperty ("command_parameters",
					new JObject (
						new JProperty ("request_user_id", requestUserId),
						new JProperty ("base64_string_photo", base64StringPhoto)
					)
				)
			);
			return json.ToString ();
		}

		public string CreateRecordVideoResponse(string requestUserId, string base64StringVideo){
			JObject json = CreateBaseRequestMessage (Command.Device.RecordVideo.Response);
			json.Add (
				new JProperty ("command_parameters",
					new JObject (
						new JProperty ("request_user_id", requestUserId),
						new JProperty ("base64_string_video", base64StringVideo)
					)
				)
			);
			return json.ToString ();
		}


		public string ProcessConnectResponse(JObject json){
			return (string)json ["status"];
		}

		public string ProcessDeviceInfoResponse(JObject json){
			return (string)json ["client_info"]["name"];
		}

		public string ProcessChangeDeviceNameResponse(JObject json){
			return (string)json ["client_info"]["name"];
		}

		public List<RequestItem> ProcessDeviceRequestsListResponse(JObject json){
			List<RequestItem> requests = new List<RequestItem> ();
			List<JObject> requestsJson = (json ["command_parameters"]["requests"]).Select(t => (JObject)t).ToList();
			requestsJson.ToList().ForEach(t => {
				int id = int.Parse ((string)t ["id"]);
				string email = (string)t ["user_email"];
				requests.Add(new RequestItem { 
					DeviceUserRequestId = id,
					UserEmail = email
				});
			});
			return requests;
		}

		public string ProcessDeviceAcceptRequestResponse(JObject json){
			return (string)json ["status"];
		}

		public string ProcessDeviceRejectRequestResponse(JObject json){
			return (string)json ["status"];
		}
	}
}

