require 'faye/websocket'
require 'json'

module ControlMyDevice
  class WebSocketBackend
    KEEPALIVE_TIME = 15 # in seconds

    def initialize(app)
      @app     = app
      @clients = []
      @client_infos = Hash.new
    end

    def call(env)
      if Faye::WebSocket.websocket?(env)
        ws = Faye::WebSocket.new(env, nil, {ping: KEEPALIVE_TIME })
        ws.on :open do |event|
          p [:open, @app]
          @clients << ws
          webSocketClient = WebSocketClient.new
          webSocketClient.id = ws.object_id
          @client_infos[ws.object_id] = webSocketClient
        end

        ws.on :message do |event|
          @current_client = ws
          @json = JSON.parse(event.data)
          client_type = @json['client_info']['type']
          identifier = @json['identifier']
          if (client_type == 'browser')
            user_id = authenticate_user(identifier)
            if user_id != 0
              process_browser_incoming_message(user_id);            
            else
              forbidden_result(FORBIDDEN)
            end
          else
            if (client_type == 'android')
              device_id = authenticate_device(identifier)
              if device_id != 0
                process_device_incoming_message(device_id);            
              else
                forbidden_result(FORBIDDEN)
              end
            end
          end
        end

        ws.on :close do |event|
          p [:close, ws.object_id, event.code, event.reason]
          @client_infos.delete(ws.object_id)
          @clients.delete(ws)
          ws = nil
        end

        # Return async Rack response
        ws.rack_response

      else
        @app.call(env)
      end
    end

    #browser client
    def authenticate_user(identifier)
      session = Session.where(identifier: identifier).first
      user_id = 0
      if session != nil
        user_id = session.user_id
      end
      return user_id
    end

    def process_browser_incoming_message(user_id)
      command_name = @json['command']['name']
      p command_name
      case command_name
        when CONNECT_REQUEST
          process_browser_connect_request(user_id)
        when REMOVE_MYDEVISE_REQUEST
          process_browser_remove_mydevice_request(user_id)
        when CONNECTED_DEVICES_REQUEST
          process_browser_connected_devices_request(user_id)
        when DEVICE_GET_CONTACTS_REQUEST
          process_browser_device_get_contacts_request(user_id)
        when DEVICE_GET_CALL_HISTORY_REQUEST
          process_browser_device_get_call_history_request(user_id)
        when DEVICE_GET_LOCATION_REQUEST
          process_browser_device_get_location_request(user_id)
        when DEVICE_TAKE_PHOTO_REQUEST
          process_browser_device_take_photo_request(user_id)
        when DEVICE_RECORD_VIDEO_REQUEST
          process_browser_device_record_video_request(user_id)
      end
    end

    def process_browser_connect_request(user_id)
      @client_infos[@current_client.object_id].type = "browser"
      @client_infos[@current_client.object_id].user_id = user_id
      success_result(CONNECT_RESPONSE)
    end

    def process_browser_remove_mydevice_request(user_id)
      device_id = @json['command_parameters']['device_id']
      user_device = UserDevice.where(user_id: user_id, device_id: device_id).first
      if user_device.nil?
        forbidden_result(FORBIDDEN)
      else
        user_device.delete
        @current_client.send({
            :status => "1",
            :command => {
              :name => REMOVE_MYDEVISE_RESPONSE
            },
            :command_parameters => {
              :device_id => device_id
            }
        }.to_json)
      end
    end

    def process_browser_connected_devices_request(user_id)
      @user_devices = User.find(user_id).devices
      @connected_user_devices = @user_devices.select{|device| @client_infos.any?{|k, v| v.device_id == device.id}}  
      @current_client.send({
          :status => "1",
          :command => {
            :name => CONNECTED_DEVICES_RESPONSE
          },
          :command_parameters => {
            :device_ids => @connected_user_devices.map{|device| device.id}
          }
      }.to_json)
    end

    def process_browser_device_get_contacts_request(user_id)
      @device_id = @json['command_parameters']['device_id'].to_i
      @user_device = UserDevice.where(:user_id => user_id, :device_id => @device_id).first()
      if (@user_device.nil?)
        forbidden_result(FORBIDDEN)
      else
        @device_client_info = @client_infos.select{|k, v| v.device_id == @device_id}.map{|k, v| v}.first
        if (!@device_client_info.nil?)
          @device_client = @clients.select{|client| client.object_id == @device_client_info.id}.first()
          if (!@device_client.nil?)
            @device_client.send({
              :command => {
                :name => DEVICE_GET_CONTACTS_REQUEST
              },
              :command_parameters => {
                :request_user_id => user_id
              }
            }.to_json)
          end
        end
      end
    end

    def process_browser_device_get_call_history_request(user_id)
      @device_id = @json['command_parameters']['device_id'].to_i
      @user_device = UserDevice.where(:user_id => user_id, :device_id => @device_id).first()
      if (@user_device.nil?)
        forbidden_result(FORBIDDEN)
      else
        @device_client_info = @client_infos.select{|k, v| v.device_id == @device_id}.map{|k, v| v}.first
        if (!@device_client_info.nil?)
          @device_client = @clients.select{|client| client.object_id == @device_client_info.id}.first()
          if (!@device_client.nil?)
            @device_client.send({
              :command => {
                :name => DEVICE_GET_CALL_HISTORY_REQUEST
              },
              :command_parameters => {
                :request_user_id => user_id
              }
            }.to_json)
          end
        end
      end
    end

    def process_browser_device_get_location_request(user_id)
      @device_id = @json['command_parameters']['device_id'].to_i
      @user_device = UserDevice.where(:user_id => user_id, :device_id => @device_id).first()
      if (@user_device.nil?)
        forbidden_result(FORBIDDEN)
      else
        @device_client_info = @client_infos.select{|k, v| v.device_id == @device_id}.map{|k, v| v}.first
        if (!@device_client_info.nil?)
          @device_client = @clients.select{|client| client.object_id == @device_client_info.id}.first()
          if (!@device_client.nil?)
            @device_client.send({
              :command => {
                :name => DEVICE_GET_LOCATION_REQUEST
              },
              :command_parameters => {
                :request_user_id => user_id
              }
            }.to_json)
          end
        end
      end
    end

    def process_browser_device_take_photo_request(user_id)
      @device_id = @json['command_parameters']['device_id'].to_i
      @user_device = UserDevice.where(:user_id => user_id, :device_id => @device_id).first()
      if (@user_device.nil?)
        forbidden_result(FORBIDDEN)
      else
        @device_client_info = @client_infos.select{|k, v| v.device_id == @device_id}.map{|k, v| v}.first
        if (!@device_client_info.nil?)
          @device_client = @clients.select{|client| client.object_id == @device_client_info.id}.first()
          if (!@device_client.nil?)
            @device_client.send({
              :command => {
                :name => DEVICE_TAKE_PHOTO_REQUEST
              },
              :command_parameters => {
                :request_user_id => user_id
              }
            }.to_json)
          end
        end
      end
    end

    def process_browser_device_record_video_request(user_id)
      @device_id = @json['command_parameters']['device_id'].to_i
      @user_device = UserDevice.where(:user_id => user_id, :device_id => @device_id).first()
      if (@user_device.nil?)
        forbidden_result(FORBIDDEN)
      else
        @device_client_info = @client_infos.select{|k, v| v.device_id == @device_id}.map{|k, v| v}.first
        if (!@device_client_info.nil?)
          @device_client = @clients.select{|client| client.object_id == @device_client_info.id}.first()
          if (!@device_client.nil?)
            @device_client.send({
              :command => {
                :name => DEVICE_RECORD_VIDEO_REQUEST
              },
              :command_parameters => {
                :request_user_id => user_id
              }
            }.to_json)
          end
        end
      end
    end

    #device client
    def authenticate_device(identifier)
      device_id = 0

      device = Device.where(identifier: identifier).first
      if device.nil?
        device = Device.new
        device.name = identifier
        device.identifier = identifier
        if device.save
          device_id = device.id
        end
      else
        device_id = device.id
      end
      p device_id
      return device_id
    end

    def process_device_incoming_message(device_id)
      command_name = @json['command']['name']
      p command_name
      case command_name
        when CONNECT_REQUEST
          process_device_connect_request(device_id)
        when CLIENTINFO_REQUEST
          process_device_info_request(device_id)
        when CHANGE_DEVICE_NAME_REQUEST
          process_change_device_name_request(device_id)
        when DEVICE_REQUEST_LIST_REQUEST
          process_device_request_list_request(device_id)
        when DEVICE_REQUEST_ACCEPT_REQUEST
          process_device_request_accept_request(device_id)
        when DEVICE_REQUEST_REJECT_REQUEST
          process_device_request_reject_request(device_id)
        when DEVICE_GET_CONTACTS_RESPONSE
          process_device_device_get_contacts_response(device_id)
        when DEVICE_GET_CALL_HISTORY_RESPONSE
          process_device_device_get_call_history_response(device_id)
        when DEVICE_GET_LOCATION_RESPONSE
          process_device_device_get_location_response(device_id)
        when DEVICE_TAKE_PHOTO_RESPONSE
          process_device_device_take_photo_response(device_id)
        when DEVICE_RECORD_VIDEO_RESPONSE
          process_device_device_record_video_response(device_id)
      end
    end

    def process_device_connect_request(device_id)
      @client_infos[@current_client.object_id].type = "android"
      @client_infos[@current_client.object_id].device_id = device_id
      success_result(CONNECT_RESPONSE)
    end

    def process_device_info_request(device_id)
      @client_info = @client_infos[@current_client.object_id]
      device = Device.find(@client_info.device_id)
      @current_client.send({
        :status => "1",
        :command => {
          :name => CLIENTINFO_RESPONSE
        },
        :client_info => {
          :name => device.name
        }
      }.to_json)
    end

    def process_change_device_name_request(device_id)
      newName = @json["command_parameters"]["new_name"]
      @client_info = @client_infos[@current_client.object_id]
      device = Device.find(@client_info.device_id)
      device.name = newName
      device.save
      @current_client.send({
        :status => "1",
        :command => {
          :name => CHANGE_DEVICE_NAME_RESPONSE
        },
        :client_info => {
          :name => device.name
        }
      }.to_json)
    end

    def process_device_request_list_request(device_id)
      @client_info = @client_infos[@current_client.object_id]
      @deviceUserRequestsResult = DeviceUserRequest.where(:device_id => device_id, :owner_type => 1)
      @deviceUserRequests = Array.new
      @deviceUserRequestsResult.map{|request| @deviceUserRequests.push(:id => request.id, :user_email => request.user.email) }
      @current_client.send({
        :status => "1",
        :command => {
          :name => DEVICE_REQUEST_LIST_RESPONSE
        },
        :command_parameters => {
          :requests => @deviceUserRequests
        }
      }.to_json)
    end

    def process_device_request_accept_request(device_id)
      @request_id = @json['command_parameters']['request_id']
      @request = DeviceUserRequest.where(:id => @request_id.to_i, :device_id => device_id, :owner_type => 1).first
      if @request.nil?
        forbidden_result(DEVICE_REQUEST_ACCEPT_RESPONSE)
      else
        @userDevice = UserDevice.new
        @userDevice.user_id = @request.user_id
        @userDevice.device_id = @request.device_id
        @userDevice.save
        @request.delete
        @current_client.send({
          :status => "1",
          :command => {
            :name => DEVICE_REQUEST_ACCEPT_RESPONSE
          }
        }.to_json)
      end
    end

    def process_device_request_reject_request(device_id)
      @request_id = @json['command_parameters']['request_id']
      @request = DeviceUserRequest.where(:id => @request_id.to_i, :device_id => device_id, :owner_type => 1).first
      if @request.nil?
        forbidden_result(DEVICE_REQUEST_REJECT_RESPONSE)
      else
        @request.delete
        @current_client.send({
          :status => "1",
          :command => {
            :name => DEVICE_REQUEST_REJECT_RESPONSE
          }
        }.to_json)
      end
    end

    def process_device_device_get_contacts_response(device_id)
      @user_id = @json['command_parameters']['request_user_id'].to_i
      @user_device = UserDevice.where(:user_id => @user_id, :device_id => device_id).first()
      if (@user_device.nil?)
        forbidden_result(FORBIDDEN)
      else
        @user_client_info = @client_infos.select{|k, v| v.user_id == @user_id}.map{|k, v| v}.first()
        if (!@user_client_info.nil?)
          @user_client = @clients.select{|client| client.object_id == @user_client_info.id}.first()
          if (!@user_client.nil?)
            @user_client.send({
              :command => {
                :name => DEVICE_GET_CONTACTS_RESPONSE
              },
              :command_parameters => {
                :contacts => @json['command_parameters']['contacts']
              }
            }.to_json)
          end
        end
      end
    end

    def process_device_device_get_call_history_response(device_id)
      @user_id = @json['command_parameters']['request_user_id'].to_i
      @user_device = UserDevice.where(:user_id => @user_id, :device_id => device_id).first()
      if (@user_device.nil?)
        forbidden_result(FORBIDDEN)
      else
        @user_client_info = @client_infos.select{|k, v| v.user_id == @user_id}.map{|k, v| v}.first()
        if (!@user_client_info.nil?)
          @user_client = @clients.select{|client| client.object_id == @user_client_info.id}.first()
          if (!@user_client.nil?)
            @user_client.send({
              :command => {
                :name => DEVICE_GET_CALL_HISTORY_RESPONSE
              },
              :command_parameters => {
                :call_history_items => @json['command_parameters']['call_history_items']
              }
            }.to_json)
          end
        end
      end
    end

    def process_device_device_get_location_response(device_id)
      @user_id = @json['command_parameters']['request_user_id'].to_i
      @user_device = UserDevice.where(:user_id => @user_id, :device_id => device_id).first()
      if (@user_device.nil?)
        forbidden_result(FORBIDDEN)
      else
        @user_client_info = @client_infos.select{|k, v| v.user_id == @user_id}.map{|k, v| v}.first()
        if (!@user_client_info.nil?)
          @user_client = @clients.select{|client| client.object_id == @user_client_info.id}.first()
          if (!@user_client.nil?)
            @user_client.send({
              :command => {
                :name => DEVICE_GET_LOCATION_RESPONSE
              },
              :command_parameters => {
                :location_latitude => @json['command_parameters']['location_latitude'],
                :location_longitude => @json['command_parameters']['location_longitude']
              }
            }.to_json)
          end
        end
      end
    end

    def process_device_device_take_photo_response(device_id)
      @user_id = @json['command_parameters']['request_user_id'].to_i
      @user_device = UserDevice.where(:user_id => @user_id, :device_id => device_id).first()
      if (@user_device.nil?)
        forbidden_result(FORBIDDEN)
      else
        @user_client_info = @client_infos.select{|k, v| v.user_id == @user_id}.map{|k, v| v}.first()
        if (!@user_client_info.nil?)
          @user_client = @clients.select{|client| client.object_id == @user_client_info.id}.first()
          if (!@user_client.nil?)
            @user_client.send({
              :command => {
                :name => DEVICE_TAKE_PHOTO_RESPONSE
              },
              :command_parameters => {
                :base64_string_photo => @json['command_parameters']['base64_string_photo']
              }
            }.to_json)
          end
        end
      end
    end

    def process_device_device_record_video_response(device_id)
      @user_id = @json['command_parameters']['request_user_id'].to_i
      @user_device = UserDevice.where(:user_id => @user_id, :device_id => device_id).first()
      if (@user_device.nil?)
        forbidden_result(FORBIDDEN)
      else
        @user_client_info = @client_infos.select{|k, v| v.user_id == @user_id}.map{|k, v| v}.first()
        if (!@user_client_info.nil?)
          @user_client = @clients.select{|client| client.object_id == @user_client_info.id}.first()
          if (!@user_client.nil?)
            @user_client.send({
              :command => {
                :name => DEVICE_RECORD_VIDEO_RESPONSE
              },
              :command_parameters => {
                :base64_string_video => @json['command_parameters']['base64_string_video']
              }
            }.to_json)
          end
        end
      end
    end

    #common
    def forbidden_result(command_name)
      @current_client.send({
        :status => "-1",
        :command => {
          :name => command_name
        }
      }.to_json)
    end

    def success_result(command_name)
      @current_client.send({
        :status => "1",
        :command => {
          :name => command_name
        }
      }.to_json)
    end

    FORBIDDEN = "forbiddent"

    CONNECT_REQUEST = "connect_request"
    CONNECT_RESPONSE = "connect_response"

    CLIENTINFO_REQUEST = "client_info_request"
    CLIENTINFO_RESPONSE = "client_info_response"

    REMOVE_MYDEVISE_REQUEST = "remove_mydevice_request"
    REMOVE_MYDEVISE_RESPONSE = "remove_mydevice_response"

    CHANGE_DEVICE_NAME_REQUEST = "change_device_name_request"
    CHANGE_DEVICE_NAME_RESPONSE = "change_device_name_response"

    DEVICE_REQUEST_LIST_REQUEST = "device_request_list_request"
    DEVICE_REQUEST_LIST_RESPONSE = "device_request_list_response"

    DEVICE_REQUEST_ACCEPT_REQUEST = "device_request_accept_request"
    DEVICE_REQUEST_ACCEPT_RESPONSE = "device_request_accept_response"

    DEVICE_REQUEST_REJECT_REQUEST = "device_request_reject_request"
    DEVICE_REQUEST_REJECT_RESPONSE = "device_request_reject_response"

    CONNECTED_DEVICES_REQUEST = "get_connected_devices_request"
    CONNECTED_DEVICES_RESPONSE = "get_connected_devices_response"

    DEVICE_GET_CONTACTS_REQUEST = "get_contacts_request"
    DEVICE_GET_CONTACTS_RESPONSE = "get_contacts_response"

    DEVICE_GET_CALL_HISTORY_REQUEST = "get_call_history_request"
    DEVICE_GET_CALL_HISTORY_RESPONSE = "get_call_history_response"

    DEVICE_GET_LOCATION_REQUEST = "get_location_request"
    DEVICE_GET_LOCATION_RESPONSE = "get_location_response"

    DEVICE_TAKE_PHOTO_REQUEST = "take_photo_request"
    DEVICE_TAKE_PHOTO_RESPONSE = "take_photo_response"

    DEVICE_RECORD_VIDEO_REQUEST = "record_video_request"
    DEVICE_RECORD_VIDEO_RESPONSE = "record_video_response"
  end

  class WebSocketClient
    attr_accessor :id, :type, :user_id, :device_id
  end 
end