class ApplicationController < ActionController::Base
  protect_from_forgery

  def authenticate_user
  	if current_user.nil?
  		redirect_to :log_in
  	else
      session = Session.where(user_id: current_user.id).first
      identifier = generate_identifier
      cookies[:identifier] = identifier
      session.identifier = identifier
      session.save
    end
  end

  def check_only_public_request
    if !current_user.nil?
      redirect_to :dashboard
    end
  end 

  helper_method :current_user

  private
  def current_user
    @current_user ||= User.find(session[:user_id]) if session[:user_id]
  end

  private
  def generate_identifier
    return (0...100).map{ (65 + rand(25)).chr }.join
  end
end
