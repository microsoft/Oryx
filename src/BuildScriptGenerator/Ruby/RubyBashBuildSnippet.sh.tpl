echo
echo "Using Ruby version:"
ruby --version
echo
echo
echo "Using Gem version:"
gem --version
echo

{{ if UseBundlerToInstallDependencies }}
    {{ if BundlerVersion | IsNotBlank }}
    BundlerVersion={{ BundlerVersion }}
    echo "Running 'gem install bundler:$BundlerVersion'..."
    echo
    gem install bundler:$BundlerVersion
    {{ else }}
    echo "Running 'gem install bundler'..."
    echo
    gem install bundler
    {{end}}
echo "Running 'bundle install'..."
echo
bundle install
{{ end }}