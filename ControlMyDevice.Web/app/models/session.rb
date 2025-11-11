class Session < ActiveRecord::Base
  attr_accessible :identifier, :user_id
end
