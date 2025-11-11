// This is a manifest file that'll be compiled into application.js, which will include all the files
// listed below.
//
// Any JavaScript/Coffee file within this directory, lib/assets/javascripts, vendor/assets/javascripts,
// or vendor/assets/javascripts of plugins, if any, can be referenced here using a relative path.
//
// It's not advisable to add code directly here, but if you do, it'll appear at the bottom of the
// the compiled file.
//
// WARNING: THE FIRST BLANK LINE MARKS THE END OF WHAT'S TO BE PROCESSED, ANY BLANK LINE SHOULD
// GO AFTER THE REQUIRES BELOW.
//
//= require jquery
//= require jquery_ujs
//= require_tree .

var app = {
	webSocket: null,
	clientId: null,
	clientType: "browser",
	identifier: null
}

function InitBaseWebSocket() {
	var ws = new WebSocket(WebSocketServer.url);
	ws.onopen = function() {
		console.log("WebSocket has opened");
		connectWebSocket();
	};
	ws.onclose = function() {
		console.log("WebSocket has closed");
	};
	ws.onerror = function(e) {
		console.log("WebSocket error");
	}
	app.webSocket = ws;
}

function connectWebSocket() {
	var message = createBaseWebSocketMessage(Command.Connect.Request);
	app.webSocket.send(JSON.stringify(message));
}

function createBaseWebSocketMessage(commandName) {
	var identifier = app.identifier;
	
	if (identifier == null)
		identifier = $.cookie("identifier")
	
	var message = {
		command: {
			name: commandName
		},
		identifier: identifier,
		client_info: {
			id: app.clientId,
			type: app.clientType
		}
	};
	return message;
}

function baseOnMessage(message) {
	var data = JSON.parse(message.data);
    var commandName = data.command.name;
	switch (commandName){
      case Command.Connect.Response:
        ProcessConnectResponse(data);
        return true;
    }
}

function ProcessConnectResponse(message){
  var status = message.status;
  if (status != ResponseStatus.Success) {
    console.log(message.command.name + " faild")
  } else {
  	try {
  		ProcessConnectSuccess();
  	} catch (error) {
  		console.log('After connect function does not exist')
  	}
  }
}

$( ).ready(function() {
		
});