using System;
using Android.Net;
using Android.App;
using WebSocket4Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ControlMyDevice
{
	public static class WebSocketManager
	{
		private static string WebSocketServerUrl = "wss://controlmydevice.herokuapp.com/";
		private static string _clientId;
		public static string ClientName = null;
		private static WebSocket _webSocket; 


		static WebSocketManager ()
		{
			_webSocket = new WebSocket (WebSocketServerUrl);
			_webSocket.AllowUnstrustedCertificate = true;

			_webSocket.Opened += (object sender, EventArgs e) => {
				Console.WriteLine ("WebSocket opened");
			};

			_webSocket.Error += (object sender, SuperSocket.ClientEngine.ErrorEventArgs e) => { 
				Console.WriteLine (e.ToString());
			};

			_webSocket.Closed += (object sender, EventArgs e) => { 
				Console.WriteLine("closed");
				var connectivityManager = (ConnectivityManager)Application.Context.GetSystemService (Activity.ConnectivityService);
				while (connectivityManager.ActiveNetworkInfo == null || !connectivityManager.ActiveNetworkInfo.IsConnected){
					System.Threading.Thread.Sleep(5000);
					Console.WriteLine ("No internet connection");
				}
				_webSocket.Open();
			};
			_webSocket.Open ();

			_webSocket.MessageReceived += (object sender, MessageReceivedEventArgs e) => {
				var messageProcessor = new MessageProcessor();

				JObject jsonObject = JObject.Parse (jsonMessage);
				string commandName = (string)jsonObject [JsonStructure.Command.NodeName][JsonStructure.Command.Name];

				switch (switch_on) {
				default:
				break;
				}

				messageProcessor.ProcessIncommingMessage(e.Message);
			};
		}

		public static void Init(string clientId){
			_clientId = clientId;
		}

		public static void Create() { }
	}
}

