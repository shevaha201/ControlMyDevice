(function( $ ){

  $.fn.deviceEmulator = function() {
    app.clientType = "android"
    app.identifier = "emulator"
    InitWebSocket();

    $(this).find('[data-device-emulator]').each(function() {
      $(this).find('[data-command-button]').each(function() {
        var commandName = $(this).attr('data-command-name');
        $(this).on('click', function(){
          if (commandName == "requests"){
            message = createBaseWebSocketMessage(Command.Emulator.RequestList.Request)
            app.webSocket.send(JSON.stringify(message));
          }
          if (commandName == "accept"){
            var requestId = $(this).attr('data-request-id');
            message = createBaseWebSocketMessage(Command.Emulator.AcceptRequest.Request)
            message.command_parameters = { requestId: requestId };
            app.webSocket.send(JSON.stringify(message));
          }
          if (commandName == "reject"){
            var requestId = $(this).attr('data-request-id');
            message = createBaseWebSocketMessage(Command.Emulator.RejectRequest.Request)
            message.command_parameters = { requestId: requestId };
            app.webSocket.send(JSON.stringify(message));
          }
        })
      })
    });

    function initButtons(scope){
      $this = $('[data-device-emulator]');
      $this.find('[data-command-button]').each(function() {
        var commandName = $(this).attr('data-command-name');
        $(this).on('click', function(){
          if (commandName == "requests"){
            message = createBaseWebSocketMessage(Command.Emulator.RequestList.Request)
            app.webSocket.send(JSON.stringify(message));
          }
          if (commandName == "accept"){
            var requestId = $(this).parent('[data-request-item]').attr('data-request-id');
            message = createBaseWebSocketMessage(Command.Emulator.AcceptRequest.Request)
            message.command_parameters = { request_id: requestId };
            app.webSocket.send(JSON.stringify(message));
          }
          if (commandName == "reject"){
            var requestId = $(this).parent('[data-request-item]').attr('data-request-id');
            message = createBaseWebSocketMessage(Command.Emulator.RejectRequest.Request)
            message.command_parameters = { request_id: requestId };
            app.webSocket.send(JSON.stringify(message));
          }
        })
      })
    }

    function InitWebSocket(){
      InitBaseWebSocket();
      var ws = app.webSocket;
      ws.onmessage = function(message) {
        var data = JSON.parse(message.data);
        var commandName = data.command.name;
        if (baseOnMessage(message) == undefined) {
          switch (commandName){
            case Command.Emulator.RequestList.Response:
              ProccessDeviceRequestList(data);
              initButtons();
              break;
            case Command.Emulator.AcceptRequest.Response:
              ProccessDeviceAcceptRequest(data);
              break;
            case Command.Emulator.RejectRequest.Response:
              ProccessDeviceRejectRequest(data);
              break;
            default:
              console.log("Bad command");
              break;
          }
        }
      }
    }

    function ProccessDeviceRequestList(message){
      var htmlString = "";
      var requests = message.command_parameters.requests
      var requestCount = requests.length
      for (var i = 0; i < requestCount; i++){
        htmlString += 
              "<div data-request-item data-request-id='" + requests[i].id + "''>" + 
                "<div>Name:" + requests[i].user_email + "</div>" + 
                "<button class='btn btn-info' data-command-button data-command-name='accept'>Accept</button>" +
                "<button class='btn btn-info' data-command-button data-command-name='reject'>Reject</button>" +
              "<div>"
      } 
      $('#requests').html(htmlString);
    }

    function ProccessDeviceAcceptRequest(message){
      alert('accepted');
    }

    function ProccessDeviceRejectRequest(message){
      alert('rejected');
    }
  };
})( jQuery );

$("#deviceEmulator").deviceEmulator();