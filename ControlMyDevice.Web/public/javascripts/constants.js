var JsonStructure = {
	Command: {
		NodeName: "Command",
		Name: "Name"
	}
};

var Command = {
	Connect: { 
		Request: "connect_request",
		Response: "connect_response"
	},
	RemoveMyDevice: {
		Request: "remove_mydevice_request",
		Response: "remove_mydevice_response"
	},
	ConnectedDevices: {
		Request: "get_connected_devices_request",
		Response: "get_connected_devices_response"
	},
	Device: {
		GetContacts: {
			Request: "get_contacts_request",
			Response: "get_contacts_response"
		},
		GetCallHistory: {
			Request: "get_call_history_request",
			Response: "get_call_history_response"
		},
		GetLocation: {
			Request: "get_location_request",
			Response: "get_location_response"
		},
		TakePhoto: {
			Request: "take_photo_request",
			Response: "take_photo_response"
		},
		RecordVideo: {
			Request: "record_video_request",
			Response: "record_video_response"
		}
	},
	Emulator: {
		RequestList: {
			Request: "device_request_list_request",
			Response: "device_request_list_response"
		},
		AcceptRequest: {
			Request: "device_request_accept_request",
			Response: "device_request_accept_response"
		},
		RejectRequest: {
			Request: "device_request_reject_request",
			Response: "device_request_reject_response"
		}
	}
};

var WebSocketServer = {
	//url: "ws://0.0.0.0:9292"
	url: "wss://controlmydevice.herokuapp.com"
}

var ResponseStatus = {
	Success: "1",
	Forbiddent: "-1"
}