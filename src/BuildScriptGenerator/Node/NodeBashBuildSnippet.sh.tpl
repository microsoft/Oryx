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



{{ if YarnVersionSpec | IsNotBlank }}
echo
echo "Found yarn version spec to follow: '{{ YarnVersionSpec }}'"
echo "Updating version of yarn installed to meet the above version spec."
yarn set version '{{ YarnVersionSpec }}'
echo
{{ else if NpmVersionSpec | IsNotBlank }}
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

{{ if YarnTimeOutConfig | IsNotBlank }}
echo
echo "Found yarn network timeout config."
echo "Setting it up with command: yarn config set network-timeout {{ YarnTimeOutConfig }} -g"
yarn config set network-timeout {{ YarnTimeOutConfig }} -g
{{ end }}

zippedModulesFileName={{ CompressedNodeModulesFileName }}
allModulesDirName=".oryx_all_node_modules"
prodModulesDirName=".oryx_prod_node_modules"
PruneDevDependencies={{ PruneDevDependencies }}
HasProdDependencies={{ HasProdDependencies }}
HasDevDependencies={{ HasDevDependencies }}
packageDirName={{ PackageDirectory }}

{{ if DependencyResolutionRequired }}
clear_dependency_resolution_artifacts() {
	local output_dir=$1

	if [ -z "$output_dir" ]; then
		return
	fi

	rm -f -- \
		"$output_dir/dependency-resolution-metadata.json" \
		"$output_dir/dependency-resolution.json" \
		"$output_dir/dependency-resolution.txt"
}

publish_dependency_resolution() {
	local output_dir=$1
	local resolution_file_name="dependency-resolution.json"

	if ! mkdir -p -- "$output_dir"; then
		return 1
	fi

	(
		local metadata_file_name="dependency-resolution-metadata.json"
		local staging_suffix=".$$.${RANDOM}.tmp"
		local raw_npm_tree
		local staged_resolution_file="$output_dir/$resolution_file_name$staging_suffix"
		local staged_metadata_file="$output_dir/$metadata_file_name$staging_suffix"
		local resolution_file="$output_dir/$resolution_file_name"
		local metadata_file="$output_dir/$metadata_file_name"

		raw_npm_tree=$(mktemp 2> /dev/null) || exit 1
		trap 'rm -f -- "$raw_npm_tree" "$staged_resolution_file" "$staged_metadata_file"' EXIT

		# npm --production excludes development-only packages while retaining the
		# actual installed tree, including nested duplicate package versions.
		npm ls --all --production --json > "$raw_npm_tree" 2> /dev/null || exit 1

		node - \
			"$raw_npm_tree" \
			"$staged_resolution_file" \
			"$staged_metadata_file" \
			"$resolution_file" <<'NODE' || exit 1
const fs = require('fs');

const [
  sourcePath,
  stagedResolutionPath,
  stagedMetadataPath,
  resolutionPath,
] = process.argv.slice(2);
const packagesByKey = new Map();

JSON.parse(fs.readFileSync(sourcePath, 'utf8'), (fallbackName, dependency) => {
  if (fallbackName && typeof dependency?.version === 'string') {
    const name =
      typeof dependency.name === 'string' ? dependency.name : fallbackName;
    packagesByKey.set(
      JSON.stringify([name, dependency.version]),
      { name, version: dependency.version });
  }

  return dependency;
});

function writeJson(path, value) {
  fs.writeFileSync(
    path,
    `${JSON.stringify(value, null, 2)}\n`,
    { encoding: 'utf8', mode: 0o600 });
}

const packages = [...packagesByKey.values()];
packages.sort(
  (left, right) =>
    left.name === right.name
      ? left.version < right.version ? -1 : left.version > right.version ? 1 : 0
      : left.name < right.name ? -1 : 1);

writeJson(stagedResolutionPath, { schemaVersion: 1, packages });
writeJson(stagedMetadataPath, {
  schemaVersion: 1,
  manager: 'npm',
  dependencyResolutionFilePath: resolutionPath,
});
NODE

		rm -f -- "$metadata_file"
		mv -f -- "$staged_resolution_file" "$resolution_file" || exit 1
		mv -f -- "$staged_metadata_file" "$metadata_file" || {
			rm -f -- "$resolution_file"
			exit 1
		}

		rm -f -- "$output_dir/dependency-resolution.txt"
	)
}

dependencyResolutionOutputDir={{ DependencyResolutionOutputDirBashValue }}
if ! clear_dependency_resolution_artifacts "$dependencyResolutionOutputDir"; then
	echo "Oryx dependency resolution artifacts could not be cleared from '$dependencyResolutionOutputDir'; deployment will continue."
fi
{{ end }}

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

	{{ if YarnVersionSpec | IsNotBlank }}
	if [ -f "$SOURCE_DIR/.yarn/releases/yarn-{{ YarnVersionSpec }}"* ]; then
		cp -f "$SOURCE_DIR/.yarn/releases/yarn-{{ YarnVersionSpec }}"* .
	fi
	{{ end }}
	
	echo
	echo "Installing production dependencies in '$SOURCE_DIR/$prodModulesDirName'..."
	echo
	echo "Running '{{ ProductionOnlyPackageInstallCommand }}'..."
	echo
	printf %s ", {{ ProductionOnlyPackageInstallCommand }}" >> "$COMMAND_MANIFEST_FILE"
	START_TIME=$SECONDS
	{{ ProductionOnlyPackageInstallCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Installing production dependencies done in $ELAPSED_TIME sec(s)."
	

	if [ -d "node_modules" ]; then
		echo
		echo "Copying production dependencies from '$SOURCE_DIR/$prodModulesDirName' to '$SOURCE_DIR/node_modules'..."
		START_TIME=$SECONDS
		rsync -rcE --links "node_modules/" "$SOURCE_DIR/node_modules"
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Copying production dependencies done in $ELAPSED_TIME sec(s)."
	fi
fi

# ensure that if the current user is root, that the root user also owns
# the application directory. This ensures that when npm install runs, it 
# does so as the root user, and therefore can access the npm cache located at
# ~/.npm by default
if [[ "$(whoami)" == "root" ]]; then
	chown -R root:root $SOURCE_DIR
fi

cd "$SOURCE_DIR"

{{ if CustomBuildCommand | IsNotBlank }}
	echo
	echo "Running '{{ CustomBuildCommand }}'..."
	echo
	printf %s ", {{ CustomBuildCommand }}" >> "$COMMAND_MANIFEST_FILE"
	START_TIME=$SECONDS
	{{ CustomBuildCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Custom build command done in $ELAPSED_TIME sec(s)."
{{ else if CustomRunBuildCommand | IsNotBlank }}
	echo
	echo "Running '{{ PackageInstallCommand }}'..."
	echo
	printf %s ", {{ PackageInstallCommand }}" >> "$COMMAND_MANIFEST_FILE"
	START_TIME=$SECONDS
	{{ PackageInstallCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Package install done in $ELAPSED_TIME sec(s)."
	echo
	printf %s ", {{ CustomRunBuildCommand }}" >> "$COMMAND_MANIFEST_FILE"
	START_TIME=$SECONDS
	{{ CustomRunBuildCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Custom run build command done in $ELAPSED_TIME sec(s)."
	echo
{{ else if LernaRunBuildCommand | IsNotBlank }}
	echo
	echo "Using Lerna version:"
	lerna --version
	echo
	echo
	echo "Running '{{ LernaInitCommand }} & {{ LernaBootstrapCommand }}':"
	printf %s ", {{ LernaInitCommand }}', '{{ LernaBootstrapCommand }}" >> "$COMMAND_MANIFEST_FILE"
	START_TIME=$SECONDS
	{{ LernaInitCommand }}
	{{ LernaBootstrapCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Lerna init and bootstrap done in $ELAPSED_TIME sec(s)."
	echo
	echo
	echo "Running '{{ LernaRunBuildCommand }}'..."
	echo
	printf %s ", {{ LernaRunBuildCommand }}" >> "$COMMAND_MANIFEST_FILE"
	START_TIME=$SECONDS
	{{ LernaRunBuildCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Lerna run build command done in $ELAPSED_TIME sec(s)."
{{ else if LageRunBuildCommand | IsNotBlank }}
	echo
	echo "Running ' {{ InstallLageCommand }} ':"
	printf %s ", {{ InstallLageCommand }}" >> "$COMMAND_MANIFEST_FILE"
	START_TIME=$SECONDS
	{{ InstallLageCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Installing Lage done in $ELAPSED_TIME sec(s)."
	echo
	echo
	echo "Running '{{ LageRunBuildCommand }}'..."
	printf %s ", {{ LageRunBuildCommand }}" >> "$COMMAND_MANIFEST_FILE"
	echo
	START_TIME=$SECONDS
	{{ LageRunBuildCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Lage run build command done in $ELAPSED_TIME sec(s)."
{{ else }}
	echo
	echo "Running '{{ PackageInstallCommand }}'..."
	echo
	printf %s ", {{ PackageInstallCommand }}" >> "$COMMAND_MANIFEST_FILE"
	START_TIME=$SECONDS
	{{ PackageInstallCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "Package install done in $ELAPSED_TIME sec(s)."
	{{ if NpmRunBuildCommand | IsNotBlank }}
	echo
	echo "Running '{{ NpmRunBuildCommand }}'..."
	printf %s ", {{ NpmRunBuildCommand }}" >> "$COMMAND_MANIFEST_FILE"
	echo
	START_TIME=$SECONDS
	{{ NpmRunBuildCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "npm run build done in $ELAPSED_TIME sec(s)."
	{{ end }}
	{{ if NpmRunBuildAzureCommand | IsNotBlank }}
	echo
	echo "Running '{{ NpmRunBuildAzureCommand }}'..."
	printf %s ", {{ NpmRunBuildAzureCommand }}" >> "$COMMAND_MANIFEST_FILE"
	echo
	START_TIME=$SECONDS
	{{ NpmRunBuildAzureCommand }}
	ELAPSED_TIME=$(($SECONDS - $START_TIME))
	echo "npm run build (Azure) done in $ELAPSED_TIME sec(s)."
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
START_TIME=$SECONDS
npm run package || true
npm run prepublishOnly || true
ELAPSED_TIME=$(($SECONDS - $START_TIME))
echo "Custom packaging scripts done in $ELAPSED_TIME sec(s)."
printf %s ", npm run package || true, npm run prepublishOnly || true" >> "$COMMAND_MANIFEST_FILE"
echo
echo "Running 'npm pack'..."
echo
printf %s ", npm pack" >> "$COMMAND_MANIFEST_FILE"
START_TIME=$SECONDS
npm pack
ELAPSED_TIME=$(($SECONDS - $START_TIME))
echo "npm pack done in $ELAPSED_TIME sec(s)."
{{ end }}


ReadImageType=$(cat /opt/oryx/.imagetype)

if [ "$ReadImageType" = "vso-focal" ] || [ "$ReadImageType" = "vso-debian-bullseye" ]
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

{{ if DependencyResolutionRequired }}
if publish_dependency_resolution "$dependencyResolutionOutputDir"; then
	echo "Oryx dependency resolution artifacts written to '$dependencyResolutionOutputDir'."
else
	echo "Oryx dependency resolution artifacts could not be written to '$dependencyResolutionOutputDir'; deployment will continue."
fi
{{ end }}

{{ if CompressNodeModulesCommand | IsNotBlank }}
if [ "$SOURCE_DIR" != "$DESTINATION_DIR" ]
then
	if [ -f $zippedModulesFileName ]; then
		echo
		echo "File '$zippedModulesFileName' already exists under '$SOURCE_DIR'. Deleting it..."
		rm -f $zippedModulesFileName
	fi

	# Keep only the current node_modules tarball in the output directory. If a previous build
	# produced the other codec (gzip vs zstd), remove that stale archive
	for staleModulesArchive in node_modules.tar.gz node_modules.tar.zst; do
		if [ "$staleModulesArchive" != "$zippedModulesFileName" ] && [ -f "$DESTINATION_DIR/$staleModulesArchive" ]; then
			echo "Removing stale node_modules archive '$DESTINATION_DIR/$staleModulesArchive'..."
			rm -f "$DESTINATION_DIR/$staleModulesArchive"
		fi
	done

	if [ -d node_modules ]
	then
		echo
		echo "Archiving existing 'node_modules' folder into '$zippedModulesFileName'..."
		START_TIME=$SECONDS
		# Make the contents of the node_modules folder appear in the zip file, not the folder itself
		cd node_modules
		{{ CompressNodeModulesCommand }} ../$zippedModulesFileName .
		ELAPSED_TIME=$(($SECONDS - $START_TIME))
		echo "Archiving node_modules done in $ELAPSED_TIME sec(s)."
	fi
fi
{{ end }}
