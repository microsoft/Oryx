echo
echo "Using Ruby version:"
ruby --version
echo
echo
echo "Using Gem version:"
gem --version
echo

{{ if UseBundlerToInstallDependencies }}
echo "Running 'gem install bundler'..."
echo
gem install bundler
echo "Running 'bundle install'..."
echo
bundle install
{{ end }}