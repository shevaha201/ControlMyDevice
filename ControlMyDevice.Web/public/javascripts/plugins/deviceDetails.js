(function( $ ){

  var pluginData = {
    $commandResult: null
  }

  $.fn.deviceDetails = function() {
    InitWebSocket();
    pluginData.$CommandResult = $(this).find('#commandResult')

    $(this).find('[data-device-details]').each(function() {
      var item = { deviceId: null };
      item.deviceId = $(this).find("[data-device-id]").attr("data-device-id");
        
      $(this).find("[data-command-button]").on("click", function() {
        var commandName = $(this).attr("data-command-name");
        var command = ""
        switch (commandName) {
          case "get-device-contacts":
            command = Command.Device.GetContacts.Request;
            break;
          case "get-device-call-history":
            command = Command.Device.GetCallHistory.Request;
            break;
          case "get-device-location":
            command = Command.Device.GetLocation.Request;
            break;
          case "take-photo":
            command = Command.Device.TakePhoto.Request;
            break;
          case "record-video":
            command = Command.Device.RecordVideo.Request;
            break;
          default:
            break;
        }
        if (command != "") {
          var parameters = { 
            commandName: command,
            deviceId: item.deviceId
          }
          message = createBaseWebSocketMessage(parameters.commandName);
          message.command_parameters = {
            device_id: parameters.deviceId
          };
          app.webSocket.send(JSON.stringify(message));
          $('[data-command-button]').attr('disabled', true)
        }
      })
    });
    function InitWebSocket(){
      InitBaseWebSocket();
      var ws = app.webSocket;
      ws.onmessage = function(message) {
        var data = JSON.parse(message.data);
        var commandName = data.command.name;
        if (baseOnMessage(message) == undefined) {
          $('[data-command-button]').attr('disabled', false);
          switch (commandName) {
            case Command.Device.GetContacts.Response:
              ProcessGetContactsResponse(data);
              break;
            case Command.Device.GetCallHistory.Response:
              ProcessGetCallHistoryResponse(data);
              break;
            case Command.Device.GetLocation.Response:
              ProcessGetLocationResponse(data);
              break;
            case Command.Device.TakePhoto.Response:
              ProcessTakePhotoResponse(data);
              break;
            case Command.Device.RecordVideo.Response:
              ProcessRecordVideoResponse(data);
              break;
            default:
              console.log("Bad command");
              break;
          }
        }
      }
    }

    function ProcessGetContactsResponse(message){
      var contacts = JSON.parse(message.command_parameters.contacts);
      var count = contacts.length;
      var resultHtml = "<h3>Device contacts</h3><ul class='list-result'>"
      for (var i = 0; i < count; i++){
        resultHtml += "<li>" + contacts[i].Name + ": " + contacts[i].Number + "</li>"
      }
      resultHtml += "</ul>";
      pluginData.$CommandResult.html(resultHtml);
    }

    function ProcessGetCallHistoryResponse(message){
      var callHistory = JSON.parse(message.command_parameters.call_history_items);
      var callHistoryCount = callHistory.length;
      var incomingCalls = [];
      var outgoingCalls = [];

      for (var i = 0; i < callHistory.length; i++) {
        callHistory[i].DateTime = new Date(callHistory[i].DateTime);
        if (callHistory[i].Type == "incomming")
          incomingCalls.push(callHistory[i]);
        if (callHistory[i].Type == "outgoing")
          outgoingCalls.push(callHistory[i]);
      }

      var resultHtml = "<h3>Call history</h3>"
      incomingCalls = incomingCalls.sort(compare);
      var incomingCount = incomingCalls.length;
      resultHtml += "<h4>Incoming</h4><ul class='list-result'>"
      for (var i = 0; i < incomingCount; i++){
        resultHtml += "<li>" + incomingCalls[i].Name + ": " + incomingCalls[i].Number + "(" + incomingCalls[i].DateTime.toLocaleString() + ")" + "</li>"
      }
      resultHtml += "</ul>";

      outgoingCalls = outgoingCalls.sort(compare)
      var outgoingCount = outgoingCalls.length;
      resultHtml += "<h4>Outgoing</h4><ul class='list-result'>"
      for (var i = 0; i < outgoingCount; i++){
        resultHtml += "<li>" + outgoingCalls[i].Name + ": " + outgoingCalls[i].Number + "(" + outgoingCalls[i].DateTime.toLocaleString() + ")" + "</li>"
      }
      resultHtml += "</ul>";
      pluginData.$CommandResult.html(resultHtml);
    }

    function ProcessGetLocationResponse(message){
      var locationLatitude = message.command_parameters.location_latitude;
      var locationLongitude = message.command_parameters.location_longitude;
      var address = locationLatitude + ", " + locationLongitude;
      pluginData.$CommandResult.html("<div id='deviceLocation' class='map'></div>")
      ShowLocation(address, "deviceLocation");
    }

    function ProcessTakePhotoResponse(message){
      var base64StringPhoto = message.command_parameters.base64_string_photo;
      pluginData.$CommandResult.html("<img class='media-result' src='data:image/jpg;base64," + base64StringPhoto + "'/>")
    }

    function ProcessRecordVideoResponse(message){
      var base64StringVideo = message.command_parameters.base64_string_video;
      pluginData.$CommandResult.html("<video controls class='media-result'> <source type='video/mp4' src='data:video/mp4;base64," + base64StringVideo+ "'/></video>")
    }

    function compare(first, second) {
      if (first.DateTime < second.DateTime)
        return 1;
      if (first.DateTime > second.DateTime)
        return -1;
      return 0;
    }

    function ShowLocation(address, id) {
        var geocoder = new google.maps.Geocoder();
        geocoder.geocode( { "address": address }, function(results, status) {
          if (status == google.maps.GeocoderStatus.OK) {
            var options = {
              zoom: 16,
              center: results[0].geometry.location,
              mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            var map = new google.maps.Map(document.getElementById(id), options);
            var marker = new google.maps.Marker({
              map: map,
              position: results[0].geometry.location
            });
          } else {
            try {
              console.error("Geocode was not successful for the following reason: " + status);
            } catch(e) {}
          }
        });
    }

  };
})( jQuery );

$("#deviceDetails").deviceDetails();