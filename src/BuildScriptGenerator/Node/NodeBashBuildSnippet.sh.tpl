{{ if NodeBuildCommandsFile | IsNotBlank }}
COMMAND_MANIFEST_FILE="{{ NodeBuildCommandsFile }}"

echo "Removing existing manifest file"
rm -f "$COMMAND_MANIFEST_FILE"
echo "Creating directory for command manifest file if it does not exist"
mkdir -p "$(dirname "$COMMAND_MANIFEST_FILE")"

{{ if NodeBuildProperties != empty }}
echo "Creating a manifest file..."
{{ for prop in NodeBuildProperties }}
echo "{{ prop.Key }}={{ prop.Value }}" >> "$COMMAND_MANIFEST_FILE"
{{ end }}
echo "Node Build Command Manifest file created."
{{ end }}
{{ end }}

doc="https://docs.microsoft.com/en-us/azure/app-service/configure-language-nodejs?pivots=platform-linux#troubleshooting"

{{ if NpmVersionSpec | IsNotBlank }}
echo
echo "Found npm version spec to follow in package.json: '{{ NpmVersionSpec }}'"
echo "Updating version of npm installed to meet the above version spec."
npm install -g npm@'{{ NpmVersionSpec }}'
echo
{{ end }}

echo
echo "Using Node version:"
node --version
echo
echo "BuildCommands=" >> "$COMMAND_MANIFEST_FILE"
{{ PackageInstallerVersionCommand }}

{{ if PackageRegistryUrl | IsNotBlank }}
echo
echo "Adding package registry to .npmrc: {{ PackageRegistryUrl }}"
echo "registry={{ PackageRegistryUrl }}" >> ~/.npmrc
echo
{{ end }}

zippedModulesFileName={{ CompressedNodeModulesFileName }}
allModulesDirName=".oryx_all_node_modules"
prodModulesDirName=".oryx_prod_node_modules"
PruneDevDependencies={{ PruneDevDependencies }}
HasProdDependencies={{ HasProdDependencies }}
HasDevDependencies={{ HasDevDependencies }}
packageDirName={{ PackageDirectory }}

# if node modules exist separately for dev & prod (like from an earlier build),
# rename the folders back appropriately for the current build
if [ -d "$allModulesDirName" ]
then
	echo
	echo "Found existing folder '$SOURCE_DIR/$allModulesDirName'."
	echo "Copying modules from '$SOURCE_DIR/$allModulesDirName' to '$SOURCE_DIR/node_modules'..."
	cd "$SOURCE_DIR"
	mkdir -p node_modules
	rsync -rcE --links "$allModulesDirName/" node_modules
fi

if [ "$PruneDevDependencies" == "true" ] && [ "$HasProdDependencies" == "true" ]
then
	# Delete existing prod modules folder so that we do not publish
	# any unused modules to final destination directory.
	if [ -d "$prodModulesDirName" ]; then
		echo "Found existing '$SOURCE_DIR/$prodModulesDirName'. Deleting it..."
		rm -rf "$prodModulesDirName"
	fi

	mkdir -p "$prodModulesDirName"
	cd "$prodModulesDirName"

	if [ -f "$SOURCE_DIR/package.json" ]; then
		cp -f "$SOURCE_DIR/package.json" .
	fi

	if [ -f "$SOURCE_DIR/package-lock.json" ]; then
		cp -f "$SOURCE_DIR/package-lock.json" .
	fi
	
	if [ -f "$SOURCE_DIR/.npmrc" ]; then
		cp -f "$SOURCE_DIR/.npmrc" .
	fi

	if [ -f "$SOURCE_DIR/yarn.lock" ]; then
		cp -f "$SOURCE_DIR/yarn.lock" .
	fi

	if [ -f "$SOURCE_DIR/.yarnrc" ]; then
		cp -f "$SOURCE_DIR/.yarnrc" .
	fi

	if [ -f "$SOURCE_DIR/.yarnrc.yml" ]; then
		cp -f "$SOURCE_DIR/.yarnrc.yml" .
	fi

	echo
	echo "Installing production dependencies in '$SOURCE_DIR/$prodModulesDirName'..."
	echo
	echo "Running '{{ ProductionOnlyPackageInstallCommand }}'..."
	echo
	printf %s ", {{ ProductionOnlyPackageInstallCommand }}" >> "$COMMAND_MANIFEST_FILE"
	{{ ProductionOnlyPackageInstallCommand }}
	

	if [ -d "node_modules" ]; then
		echo
		echo "Copying production dependencies from '$SOURCE_DIR/$prodModulesDirName' to '$SOURCE_DIR/node_modules'..."
		START_TIME=$SECONDS
		rsync -rcE --links "node_modules/" "$SOURCE_DIR/node_modules"
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."
	fi
fi

cd "$SOURCE_DIR"

{{ if CustomBuildCommand | IsNotBlank }}
	echo
	echo "Running '{{ CustomBuildCommand }}'..."
	echo
	printf %s ", {{ CustomBuildCommand }}" >> "$COMMAND_MANIFEST_FILE"
	{{ CustomBuildCommand }}
{{ else if CustomRunBuildCommand | IsNotBlank }}
	echo
	echo "Running '{{ PackageInstallCommand }}'..."
	echo
	printf %s ", {{ PackageInstallCommand }}" >> "$COMMAND_MANIFEST_FILE"
	{{ PackageInstallCommand }}
	echo
	printf %s ", {{ CustomRunBuildCommand }}" >> "$COMMAND_MANIFEST_FILE"
	{{ CustomRunBuildCommand }}
	echo
{{ else if LernaRunBuildCommand | IsNotBlank }}
	echo
	echo "Using Lerna version:"
	lerna --version
	echo
	echo
	echo "Running '{{ LernaInitCommand }} & {{ LernaBootstrapCommand }}':"
	printf %s ", {{ LernaInitCommand }}', '{{ LernaBootstrapCommand }}" >> "$COMMAND_MANIFEST_FILE"
	{{ LernaInitCommand }}
	{{ LernaBootstrapCommand }}
	echo
	echo
	echo "Running '{{ LernaRunBuildCommand }}'..."
	echo
	printf %s ", {{ LernaRunBuildCommand }}" >> "$COMMAND_MANIFEST_FILE"
	{{ LernaRunBuildCommand }}
{{ else if LageRunBuildCommand | IsNotBlank }}
	echo
	echo "Running ' {{ InstallLageCommand }} ':"
	printf %s ", {{ InstallLageCommand }}" >> "$COMMAND_MANIFEST_FILE"
	{{ InstallLageCommand }}
	echo
	echo
	echo "Running '{{ LageRunBuildCommand }}'..."
	printf %s ", {{ LageRunBuildCommand }}" >> "$COMMAND_MANIFEST_FILE"
	echo
	{{ LageRunBuildCommand }}
{{ else }}
	echo
	echo "Running '{{ PackageInstallCommand }}'..."
	echo
	printf %s ", {{ PackageInstallCommand }}" >> "$COMMAND_MANIFEST_FILE"
	{{ PackageInstallCommand }}
	{{ if NpmRunBuildCommand | IsNotBlank }}
	echo
	echo "Running '{{ NpmRunBuildCommand }}'..."
	printf %s ", {{ NpmRunBuildCommand }}" >> "$COMMAND_MANIFEST_FILE"
	echo
	{{ NpmRunBuildCommand }}
	{{ end }}
	{{ if NpmRunBuildAzureCommand | IsNotBlank }}
	echo
	echo "Running '{{ NpmRunBuildAzureCommand }}'..."
	printf %s ", {{ NpmRunBuildAzureCommand }}" >> "$COMMAND_MANIFEST_FILE"
	echo
	{{ NpmRunBuildAzureCommand }}
	{{ end }}
{{ end }}

{{ if RunNpmPack }}
	{{ if PackageDirectory | IsNotBlank }}
	echo "Switching to package directory provided: '{{ PackageDirectory }}'..."
	if [ ! -d "$SOURCE_DIR/$packageDirName" ]; then
		LogWarning "Package directory '$SOURCE_DIR/$packageDirName' does not exist. More information: ${doc}"
		exit 1
	fi
	cd "$SOURCE_DIR/$packageDirName"
	{{ end }}
echo
echo "Running custom packaging scripts that might exist..."
echo
npm run package || true
npm run prepublishOnly || true
printf %s ", npm run package || true, npm run prepublishOnly || true" >> "$COMMAND_MANIFEST_FILE"
echo
echo "Running 'npm pack'..."
echo
printf %s ", npm pack" >> "$COMMAND_MANIFEST_FILE"
npm pack
{{ end }}


ReadImageType=$(cat /opt/oryx/.imagetype)

if [ "$ReadImageType" = "vso-focal" ]
then
	echo $ReadImageType
	cat "$COMMAND_MANIFEST_FILE"
else
	rm "$COMMAND_MANIFEST_FILE"
fi

cd "$SOURCE_DIR"

if [ "$PruneDevDependencies" == "true" ] && [ "$HasDevDependencies" == "true" ]
then
	if [ -d "node_modules" ]; then
		echo
		echo "Copy '$SOURCE_DIR/node_modules' with all dependencies to '$SOURCE_DIR/$allModulesDirName'..."
		rsync -rcE --links "node_modules/" "$allModulesDirName" --delete
	fi

	if [ "$HasProdDependencies" == "true" ] && [ -d "$prodModulesDirName/node_modules/" ]; then
		echo
		echo "Copying production dependencies from '$SOURCE_DIR/$prodModulesDirName/node_modules' to '$SOURCE_DIR/node_modules'..."
		rsync -rcE --links "$prodModulesDirName/node_modules/" node_modules --delete
	else
		rm -rf "node_modules/"
	fi
fi

{{ if CompressNodeModulesCommand | IsNotBlank }}
if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	if [ -f $zippedModulesFileName ]; then
		echo
		echo "File '$zippedModulesFileName' already exists under '$SOURCE_DIR'. Deleting it..."
		rm -f $zippedModulesFileName
	fi

	if [ -d node_modules ]
	then
		echo
		echo Zipping existing 'node_modules' folder...
		START_TIME=$SECONDS
		# Make the contents of the node_modules folder appear in the zip file, not the folder itself
		cd node_modules
		{{ CompressNodeModulesCommand }} ../$zippedModulesFileName .
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Done in $ELAPSED_TIME sec(s)."
	fi
fi
{{ end }}
