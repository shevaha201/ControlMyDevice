class DevicesController < ApplicationController
	before_filter :authenticate_user

	def list
		@devices = User.find(current_user.id).devices
	end

	def new
		@device = Device.new
	end

	def create
		@device = Device.new(params[:device])
		if (@device.name.nil? or @device.name.length == 0) and (@device.identifier.nil? or @device.identifier.length == 0)
			@device.errors[:required] << "name or identifier"
			render "new"
		else
			@exist_device = Device.where(["name = ? or identifier = ?", @device.name, @device.identifier]).first
			if @exist_device.nil?
				flash.alert = "Device doesn't exist"
				render "new"
			else
				@exist_request = DeviceUserRequest.where(["user_id = ? and device_id = ?", current_user.id, @exist_device.id]).first
				if (@exist_request.nil?)
					@exist_userDevice = UserDevice.where(["user_id = ? and device_id = ?", current_user.id, @exist_device.id]).first
					if (@exist_userDevice.nil?)
						@deviceUserReqest = DeviceUserRequest.new(:user_id => current_user.id, :device_id => @exist_device.id, :owner_type => 1)
						@deviceUserReqest.save
						redirect_to :requests
					else
						@device.errors[:error] << "user has already controled this device"
						render "new"
					end
				else
					@device.errors[:error] << "user has already sent request"
					render "new"
				end
			end
		end
	end

	def update
		@device = Device.new(params[:device])
		@device.errors[:error] << "user has already controled this device"
		render "new"
	end

	def requests
		@requests = User.find(current_user.id).device_user_requests
	end

	def details
		@deviceId = params[:id]
		@userDevice = UserDevice.where(["user_id = ? and device_id = ?", current_user.id, @deviceId]).first()
		if (@userDevice.nil?)
			flash.alert = "forbidden"
			redirect_to :root
		else
			@device = Device.find(@deviceId)
		end
	end

	def accept
		@deviceUserRequest = DeviceUserRequest.find(params[:id])
		if @deviceUserRequest.nil? or @deviceUserRequest.user_id != current_user.id or @deviceUserRequest.owner_type != 2
			flash.alert = "forbidden"
		else
			@userDevice = UserDevice.where(["user_id = ? and device_id = ?", current_user.id, @deviceUserRequest.device_id]).first
			if (@userDevice.nil?)
				@deviceUserRequest.delete
				@userDevice = UserDevice.new(:user_id => current_user.id, :device_id => @deviceUserRequest.device_id)
				@userDevice.save
				flash.alert = "Accepted"
			else
				flash.alert = "User has already controled this device"
			end
		end
		redirect_to :requests
	end

	def reject
		@deviceUserRequest = DeviceUserRequest.find(params[:id])
		if @deviceUserRequest.nil? or @deviceUserRequest.user_id != current_user.id or @deviceUserRequest.owner_type != 2
			flash.alert = "forbidden"
		else
			@deviceUserRequest.delete
			flash.alert = "Rejected"
		end
		redirect_to :requests
	end

	def cancel
		@deviceUserRequest = DeviceUserRequest.find(params[:id])
		if @deviceUserRequest.nil? or @deviceUserRequest.user_id != current_user.id or @deviceUserRequest.owner_type != 1
			flash.alert = "forbidden"
		else
			@deviceUserRequest.delete
			flash.alert = "Request canceled"
		end
		redirect_to :requests
	end

	def emulator
	end
end
