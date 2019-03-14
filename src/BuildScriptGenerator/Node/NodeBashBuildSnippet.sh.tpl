echo Installing packages ...
cd "$SOURCE_DIR"
echo
echo "Running '{{ PackageInstallCommand }}' ..."
echo

# Yarn config is per user, and since the build might run with a non-root account, we make sure
# the yarn cache is set on every build.
YARN_CACHE_DIR=/usr/local/share/yarn-cache
if [ -d $YARN_CACHE_DIR ]
then
    echo "Configuring Yarn cache folder"
    yarn config set cache-folder $YARN_CACHE_DIR
fi

{{ PackageInstallCommand }}

# TODO Using this to test the change, until we have the final PR checked in.
if [ "$ENABLE_NODE_MODULES_ZIP" == "true" ]; then
	echo "Compressing node_modules folder..."
	tar -czf node_modules.zip node_modules/
fi

{{ NpmRunBuildCommand }}

{{ NpmRunBuildAzureCommand }}
