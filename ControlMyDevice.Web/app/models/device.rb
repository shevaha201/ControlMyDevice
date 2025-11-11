class Device < ActiveRecord::Base
 	attr_accessible :identifier, :name

  	validates_uniqueness_of :identifier
  	validates_uniqueness_of :name

  	has_many :user_devices
  	has_many :users, through: :user_devices

  	has_many :device_user_requests
  	has_many :user_requests, through: :device_user_requests, :source => :user
end
