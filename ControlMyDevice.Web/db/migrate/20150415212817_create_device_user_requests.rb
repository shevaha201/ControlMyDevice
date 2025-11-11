class CreateDeviceUserRequests < ActiveRecord::Migration
  def change
    create_table :device_user_requests do |t|
      t.belongs_to :user, index: true
      t.belongs_to :device, index: true
      t.integer :owner_type
      t.timestamps null: false
    end
  end
end
