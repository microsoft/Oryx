echo
echo "Using Gem version:"
gem --version
echo
echo
echo "Using Ruby version:"
ruby --version
echo

{{ if HasRailsDependencies }}
echo "Running 'gem install rails && rails -v'..."
echo
gem install rails && rails -v
{{ end }}

{{ if UseBundlerToInstallDependencies }}
echo "Running 'gem install bundler'..."
echo
gem install bundler
echo "Running 'bundle install'..."
echo
bundle install
{{ end }}

{{ if RunRakeExecuteCommand }}
echo "Running 'bundle exec rake'..."
echo
bundle exec rake
{{ end }}