using System;

namespace ControlMyDevice
{
	public static class Command{

		public static class Connect {

			public const string Request = "connect_request";
			public const string Response = "connect_response";
		}

		public static class ClientInfo {

			public const string Request = "client_info_request";
			public const string Response = "client_info_response";
		}

		public static class ChangeDeviceName {

			public const string Request = "change_device_name_request";
			public const string Response = "change_device_name_response";
		}

		public static class DeviceRequestList {

			public const string Request = "device_request_list_request";
			public const string Response = "device_request_list_response";
		}

		public static class DeviceAcceptRequest {

			public const string Request = "device_request_accept_request";
			public const string Response = "device_request_accept_response";
		}

		public static class DeviceRejectRequest {

			public const string Request = "device_request_reject_request";
			public const string Response = "device_request_reject_response";
		}

		public static class Device {

			public static class GetCallHistory {

				public const string Request = "get_call_history_request";
				public const string Response = "get_call_history_response";
			}

			public static class GetContacts {

				public const string Request = "get_contacts_request";
				public const string Response = "get_contacts_response";
			}

			public static class GetLocation {

				public const string Request = "get_location_request";
				public const string Response = "get_location_response";
			}

			public static class TakePhoto {

				public const string Request = "take_photo_request";
				public const string Response = "take_photo_response";
			}

			public static class RecordVideo {

				public const string Request = "record_video_request";
				public const string Response = "record_video_response";
			}
		}
	}
}

