# Yarn config is per user, and since the build might run with a non-root account, we make sure
# the yarn cache is set on every build.
YARN_CACHE_DIR=/usr/local/share/yarn-cache
if [ -d $YARN_CACHE_DIR ]
then
	echo
    echo "Configuring Yarn cache folder..."
    yarn config set cache-folder $YARN_CACHE_DIR
fi

allModulesDirName=__oryx_all_node_modules
prodModulesDirName=__oryx_prod_node_modules
copyOnlyProdModulesToOutput=false
zippedOutputFileName=oryx_output.tar.gz

# We want separate folders for prod modules only when the package.json has separate dependencies
hasProductionOnlyDependencies="{{ HasProductionOnlyDependencies }}"
if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ] && \
   [ "$ORYX_COPY_ONLY_PROD_MODULES_TO_OUTPUT" == "true" ] && \
   [ "$hasProductionOnlyDependencies" == "true" ]
then
	copyOnlyProdModulesToOutput=true
fi

# if node modules exist separately for dev & prod (like from an earlier build),
# rename the folders back appropriately for the current build
if [ -d $allModulesDirName ]
then
	if [ -d node_modules ]
	then
		if [ "$copyOnlyProdModulesToOutput" == "true" ]
		then
			# Rename existing node_modules back to prod modules since current build wants them separate
			mv node_modules $prodModulesDirName
		else
			rm -rf node_modules
		fi
	fi

	# Rename the folder which has all the node modules to reuse for later builds to improve perf
	mv $allModulesDirName node_modules
fi

if [ "$copyOnlyProdModulesToOutput" == "true" ]
then
	mkdir -p "$prodModulesDirName"
	cd "$prodModulesDirName"

	if [ -f "$SOURCE_DIR/package.json" ]; then
		cp -f "$SOURCE_DIR/package.json" .
	fi

	if [ -f "$SOURCE_DIR/package-lock.json" ]; then
		cp -f "$SOURCE_DIR/package-lock.json" .
	fi

	if [ -f "$SOURCE_DIR/yarn.lock" ]; then
		cp -f "$SOURCE_DIR/yarn.lock" .
	fi

	echo
	echo "Installing production dependencies in '$prodDependencies'..."
	echo "Running '{{ ProductionOnlyPackageInstallCommand }}'..."
	echo
	{{ ProductionOnlyPackageInstallCommand }}

	echo
	echo "Copying production dependencies from '$prodDependencies' to '$SOURCE_DIR'..."
	rsync -rtE --links node_modules "$SOURCE_DIR"
fi

cd "$SOURCE_DIR"

echo
echo "Running '{{ PackageInstallCommand }}'..."
echo
{{ PackageInstallCommand }}

{{ if NpmRunBuildCommand | IsNotBlank }}
echo
echo "Running '{{ NpmRunBuildCommand }}'..."
echo
{{ NpmRunBuildCommand }}
{{ end }}

{{ if NpmRunBuildAzureCommand | IsNotBlank }}
echo
echo "Running '{{ NpmRunBuildAzureCommand }}'..."
echo
{{ NpmRunBuildAzureCommand }}
{{ end }}

if [ "$copyOnlyProdModulesToOutput" == "true" ]
then
	# Rename folder having all node_modules as we want only prod dependencies
	# to be synced with destination directory
	mv node_modules $allModulesDirName

	# Rename the folder having prod modules to be the one which we want to be present in output directory	
	mv $prodModulesDirName node_modules
fi

if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	mkdir -p "$DESTINATION_DIR"
	
	excludedDirectories=""
	{{ for excludedDir in DirectoriesToExcludeFromCopyToBuildOutputDir }}
	excludedDirectories+=" --exclude={{ excludedDir }}"
	{{ end }}

	cd "$SOURCE_DIR"
	if [ "$ORYX_ZIP_ALL_OUTPUT" == "true" ]
	then
		if [ "$(ls -A $DESTINATION_DIR)" ]
		then
			echo
			echo "Destination directory is not empty. Deleting its contents..."
			rm -rf "$DESTINATION_DIR"/*
		fi
		
		echo
		echo "Zipping the contents before copy to '$DESTINATION_DIR'..."
		echo
		touch $zippedOutputFileName
		tar $excludedDirectories --exclude=$zippedOutputFileName -zcf $zippedOutputFileName .
		cp -f $zippedOutputFileName "$DESTINATION_DIR/$zippedOutputFileName"
	else
		echo
		echo "Copying files to destination directory '$DESTINATION_DIR'..."
		rsync --delete -rtE --links $excludedDirectories . "$DESTINATION_DIR"
		echo "Finished copying files to destination directory."
	fi
fi