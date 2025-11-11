using System;

namespace ControlMyDevice
{
	public static class JsonStructure
	{
		public static class ClientInfo {

			public const string NodeName = "ClientInfo";
			public const string ClientId = "ClientId";
		}

		public static class Command {

			public const string NodeName = "command";
			public const string Name = "name";
			public const string RequestClientId = "requestClientId";
		}

		public static class DeviceInfo {

			public const string NodeName = "DeviceInfo";
			public const string Name = "Name";
		}
	}
}

