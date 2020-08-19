require 'rubygems'
require 'bundler/setup'

# app.rb
require 'sinatra'

get '/' do
  'Hello Sinatra!'
end