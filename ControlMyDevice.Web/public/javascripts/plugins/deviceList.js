(function( $ ){

  $.fn.deviceList = function() {
    InitWebSocket();

    $(this).find('[data-device-item]').each(function() {
      var item = { deviceId: null };
      item.deviceId = $(this).attr("data-device-id");
        
      $(this).find("[data-command-button]").on("click", function() {
        var parameters = { 
          name: $(this).attr("data-command-name"),
          deviceId: item.deviceId
        }
        message = createBaseWebSocketMessage(Command.RemoveMyDevice.Request)
        message.command_parameters = {
          device_id: parameters.deviceId
        };
        app.webSocket.send(JSON.stringify(message));
      })
    });

  };
})( jQuery );

function InitWebSocket(){
  InitBaseWebSocket();
  var ws = app.webSocket;
  ws.onmessage = function(message) {
    var data = JSON.parse(message.data);
    var commandName = data.command.name;
    if (baseOnMessage(message) == undefined) {
      switch (commandName){
        case Command.RemoveMyDevice.Response:
          ProcessRemoveMyDeviceResponse(data);
          break;
        case Command.ConnectedDevices.Response:
          ProcessConnectedDevicesResponse(data);
          break;
        default:
          console.log("Bad command");
          break;
      }
    }
  }
}

function ProcessRemoveMyDeviceResponse(message){
  var status = message.status;
  var deviceId = message.command_parameters.device_id;
  if (status == ResponseStatus.Success) {
    $('[data-device-item][data-device-id=' + deviceId + ']').first().remove();
  } else {
    console.log(message.command.name + " faild")
  }
}

function ProcessConnectedDevicesResponse(message){
  var status = message.status;
  if (status == ResponseStatus.Success) {
    var connectedDevices = message.command_parameters.device_ids;
    var count = connectedDevices.length;
    for (var i = 0; i < count; i++) {
      var $deviceItem = $('[data-device-id=' + connectedDevices[i] + ']')
      var $status = $deviceItem.find('#connectStatus');
      $status.removeClass('not-connected');
      $status.addClass('connected');
      $status.text('(Connected)'); 
    }
  } else {
    console.log(message.command.name + " faild")
  }
}

function ProcessConnectSuccess(){
  var message = createBaseWebSocketMessage(Command.ConnectedDevices.Request)
  app.webSocket.send(JSON.stringify(message));
}

$("#deviceList").deviceList();