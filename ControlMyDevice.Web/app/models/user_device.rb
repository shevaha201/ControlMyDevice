class UserDevice < ActiveRecord::Base
  attr_accessible :device_id, :user_id

  belongs_to :user
  belongs_to :device
end
