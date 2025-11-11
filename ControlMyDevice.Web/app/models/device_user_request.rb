class DeviceUserRequest < ActiveRecord::Base
  attr_accessible :device_id, :user_id, :owner_type

  belongs_to :user
  belongs_to :device
end
