class SessionsController < ApplicationController
 	before_filter :authenticate_user, :except => [:new, :create]
  before_filter :check_only_public_request, :only => [:new, :create]

  	def new
  	end

	def create
    	user = User.authenticate(params[:email], params[:password])
    	if user
      		session[:user_id] = user.id
          identifier = generate_identifier
          Session.create(:user_id => user.id, :identifier => identifier)
      		redirect_to root_url
    	else
    		flash.now.alert = "Invalid email or password"
      		render "new"
    	end
  	end

  	def destroy
  		session[:user_id] = nil
      Session.where(user_id: @current_user.id).first().delete
  		redirect_to root_url
	end
end
