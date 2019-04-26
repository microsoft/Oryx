{{ if ConfigureYarnCache }}
# Yarn config is per user, and since the build might run with a non-root account, we make sure
# the yarn cache is set on every build.
YARN_CACHE_DIR=/usr/local/share/yarn-cache
if [ -d $YARN_CACHE_DIR ]
then
	echo
    echo "Configuring Yarn cache folder..."
    yarn config set cache-folder $YARN_CACHE_DIR
fi
{{ end }}

zippedModulesFileName={{ CompressedNodeModulesFileName }}
allModulesDirName=__oryx_all_node_modules
prodModulesDirName=__oryx_prod_node_modules
copyOnlyProdModulesToOutput=false

PruneDevDependencies={{ PruneDevDependencies }}
# We want separate folders for prod modules only when the package.json has separate dependencies
hasProductionOnlyDependencies="{{ HasProductionOnlyDependencies }}"
if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ] && \
   [ "$PruneDevDependencies" == "true" ] && \
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
	echo "Installing production dependencies in '$prodModulesDirName'..."
	echo "Running '{{ ProductionOnlyPackageInstallCommand }}'..."
	echo
	{{ ProductionOnlyPackageInstallCommand }}

	echo
	echo "Copying production dependencies from '$prodModulesDirName' to '$SOURCE_DIR'..."
	START_TIME=$SECONDS
	rsync -rtE --links node_modules "$SOURCE_DIR"
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Done in $ELAPSED_TIME sec(s)."
fi

cd "$SOURCE_DIR"

echo
echo "Running '{{ PackageInstallCommand }}'..."
echo
{{ PackageInstallCommand }}

{{ if AppInsightsInjectCommand | IsNotBlank }}
echo
echo "Preparing environment to set up Application Insights..."
echo "Running '{{ AppInsightsInjectCommand }}'..."
echo
{{ AppInsightsInjectCommand }}
echo
echo "Creating Application Insights loader file in '$DESTINATION_DIR'"
echo "// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

// Created by Oryx
try 
{
	var appInsights = require('{{ AppInsightsPackageName }}');
	if (process.env.APPINSIGHTS_INSTRUMENTATIONKEY) {

        appInsights
            .setup()
            .start();
	}
}catch (e) {
        console.log('Application Insights could not be automatically configured for this application'); 
        console.log(e);
}">{{ AppInsightsLoaderFileName }}
cat {{ AppInsightsLoaderFileName }}
{{ end }}

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

{{ if CompressNodeModulesCommand | IsNotBlank }}
if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	if [ -f $zippedModulesFileName ]; then
		echo
		echo "File '$zippedModulesFileName' already exists under '$SOURCE_DIR'. Deleting it ..."
		rm -f $zippedModulesFileName
	fi

	if [ -d node_modules ]
	then
		echo
		echo Zipping existing 'node_modules' folder ...
		START_TIME=$SECONDS
		# Make the contents of the node_modules folder appear in the zip file, not the folder itself
		cd node_modules
		{{ CompressNodeModulesCommand }} ../$zippedModulesFileName .
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."
	fi
fi
{{ end }}
