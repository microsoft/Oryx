cd "$SOURCE_DIR"

# Yarn config is per user, and since the build might run with a non-root account, we make sure
# the yarn cache is set on every build.
YARN_CACHE_DIR=/usr/local/share/yarn-cache
if [ -d $YARN_CACHE_DIR ]
then
	echo
    echo "Configuring Yarn cache folder ..."
    yarn config set cache-folder $YARN_CACHE_DIR
fi

echo
echo Installing packages ...
echo
echo "Running '{{ PackageInstallCommand }}' ..."
echo
{{ PackageInstallCommand }}

{{ if NpmRunBuildCommand | IsNotBlank }}
echo
echo "Running '{{ NpmRunBuildCommand }}' ..."
echo
{{ NpmRunBuildCommand }}
{{ end }}

{{ if NpmRunBuildAzureCommand | IsNotBlank }}
echo
echo "Running '{{ NpmRunBuildAzureCommand }}' ..."
echo
{{ NpmRunBuildAzureCommand }}
{{ end }}

{{ if ZipNodeModulesDir }}
# If source and destination are the same, then we need not zip the contents again, but if they are different
# then we want to copy the zipped node modules to destination directory as this directory could be in a shared volume.
if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	echo
	echo Zipping 'node_modules' folder ...
	rm -f "node_modules.tar.gz"
	if [ -d node_modules ]
	then
		# Make the contents of the node_modules folder appear in the zip file, not the folder itself
		cd node_modules
		tar -zcf ../node_modules.tar.gz .
	fi
fi
{{ end }}
