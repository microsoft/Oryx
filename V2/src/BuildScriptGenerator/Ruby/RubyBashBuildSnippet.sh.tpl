echo
echo "Using Ruby version:"
ruby --version
echo
echo
echo "Using Gem version:"
gem --version
echo


{{ if BundlerVersion | IsNotBlank }}
    BundlerVersion={{ BundlerVersion }}
    echo "Running 'gem install bundler:$BundlerVersion'..."
    echo
    gem install bundler:$BundlerVersion
{{ else }}
    echo "Running 'gem install bundler'..."
    echo
    gem install bundler
{{ end }}

{{ if CustomBuildCommand | IsNotBlank }}
	echo
	echo "Running '{{ CustomBuildCommand }}'..."
	echo
	{{ CustomBuildCommand }}
{{ else }}
    {{ if ConfigYmlFileExists }}
        {{ if GemfileExists }}
        echo "Running 'bundle install'..."
        echo
        bundle install
        {{ else }}
        echo "Running 'gem install jekyll'..."
        echo
        gem install jekyll
        {{ end }}
        echo "Running 'jekyll build'..."
        echo
        jekyll build
    {{ else }}
        echo "Running 'bundle install'..."
        echo
        bundle install
    {{ end }}
{{ end }}

