using System;
using System.Collections.Generic;

namespace ControlMyDevice
{
	public static class Common {

		public static class ResponseStatus {

			public const string Success = "1";
			public const string Forbidden = "-1";			
		}

		public static readonly IDictionary<string, string> CallTypes = new Dictionary<string, string> {
			{ "1", "incomming" },
			{ "2", "outgoing" },
			{ "3", "missed" }
		};

		public const string TrimFromMobileNumber = "+38";
	}
}

