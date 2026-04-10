#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Since this file is expected to be 'sourced', we expect the REPO_DIR variable
# to be supplied in the parent script sourcing this file.
source "$REPO_DIR/build/__variables.sh"
source "$REPO_DIR/build/__sdkStorageConstants.sh"

volumeHostDir="$ARTIFACTS_DIR/platformSdks"
volumeContainerDir="/tmp/sdk"
imageName="oryx/platformsdk"

blobExists() {
	local containerName="$1"
	local blobName="$2"
	local sdkStorageAccountUrl="$3"
	local inStorageAccountFile="$REPO_DIR/platforms/${containerName//-//}/versions/inStorageAccount.txt"
	local exitCode=1

	echo "Checking if blob exists..."
	if grep "$blobName" "$inStorageAccountFile"; then
		echo "Exists in storage account"
		return 0
	else
		echo "Does not exist in storage account"
		return 1
	fi
}

shouldBuildSdk() {
	local containerName="$1"
	local blobName="$2"
	local sdkStorageAccountUrl="$3"

	# return whatever exit code the following returns
	blobExists $containerName $blobName $sdkStorageAccountUrl
	exitCode=$?
	if [ "$exitCode" == 0 ]; then
		return 1
	else
		return 0
	fi
}

shouldOverwriteSdk() {
	if [ "$OVERWRITE_EXISTING_SDKS" == "true" ]; then
		return 0
	else
		return 1
	fi
}

shouldOverwritePlatformSdk() {
	local platform="$1"
	case $platform in
        "php")
            	if [ "$OVERWRITE_EXISTING_SDKS_PHP" == "true" ]; then
					return 0
				else
					return 1
				fi
	    	;;
        "php-composer")
            	if [ "$OVERWRITE_EXISTING_SDKS_PHP_COMPOSER" == "true" ]; then
					return 0
				else
					return 1
				fi
            	;;
	"java")
	    		if [ "$OVERWRITE_EXISTING_SDKS_JAVA" == "true" ]; then
					return 0
				else
					return 1
				fi
	    	;;
        "maven")
	    		if [ "$OVERWRITE_EXISTING_SDKS_MAVEN" == "true" ]; then
					return 0
				else
					return 1
				fi
	    	;;
	"nodejs")
	 		if [ "$OVERWRITE_EXISTING_SDKS_NODE" == "true" ]; then
					return 0
				else
					return 1
				fi
	    	;;
        "ruby")
		if [ "$OVERWRITE_EXISTING_SDKS_RUBY" == "true" ]; then
					return 0
				else
					return 1
				fi
		;;
	"python")
		if [ "$OVERWRITE_EXISTING_SDKS_PYTHON" == "true" ]; then
					return 0
				else
					return 1
				fi
		;;
        "golang")
		if [ "$OVERWRITE_EXISTING_SDKS_GOLANG" == "true" ]; then
					return 0
				else
					return 1
				fi
		;;
	"dotnet")
		if [ "$OVERWRITE_EXISTING_SDKS_DOTNETCORE" == "true" ]; then
					return 0
				else
					return 1
				fi
		;;
	esac
}

isDefaultVersionFile() {
	blobName="$1"
	if [[ $blobName == defaultVersion* ]]; then
		return 0
	else
		return 1
	fi
}

getSdkFromImage() {
	local imageName="$1"
	local hostVolumeDir="$2"

	mkdir -p "$hostVolumeDir"
	echo "Copying sdk file to host directory..."
	echo
	docker run \
		--rm \
		-v /$hostVolumeDir:$volumeContainerDir \
		$imageName \
		bash -c "cp -f /tmp/compressedSdk/* /tmp/sdk"
}

buildPlatform() {
	local versionFile="$1"
	local funcToCall="$2"

	# When VERSIONS_TO_BUILD_OVERRIDE is set (comma-separated list of versions),
	# only build those specific versions and skip blob existence checks entirely.
	# This allows force-building specific SDK versions without rebuilding everything.
	if [ -n "$VERSIONS_TO_BUILD_OVERRIDE" ]; then
		echo "VERSIONS_TO_BUILD_OVERRIDE is set: $VERSIONS_TO_BUILD_OVERRIDE"
		echo "Building only specified versions, skipping storage account checks."
		export OVERWRITE_EXISTING_SDKS="true"

		# Build a lookup set of requested versions
		IFS=',' read -ra _requested_versions <<< "$VERSIONS_TO_BUILD_OVERRIDE"
		declare -A _force_set
		for _v in "${_requested_versions[@]}"; do
			_v="$(echo "$_v" | xargs)"
			[ -n "$_v" ] && _force_set["$_v"]=1
		done

		# Read the version file but only invoke the build function for matching versions.
		# This preserves extra args (e.g. GPG keys, SHAs) that some platforms need.
		while IFS= read -r VERSION_INFO || [[ -n $VERSION_INFO ]]; do
			VERSION_INFO="$(echo -e "${VERSION_INFO}" | sed -e 's/^[[:space:]]*//')"
			if [ -z "$VERSION_INFO" ] || [[ $VERSION_INFO = \#* ]]; then
				continue
			fi

			IFS=',' read -ra VERSION_INFO_PARTS <<< "$VERSION_INFO"
			lineVersion="$(echo -e "${VERSION_INFO_PARTS[0]}" | sed -e 's/^[[:space:]]*//')"

			if [ -z "${_force_set[$lineVersion]:-}" ]; then
				continue
			fi

			echo "Force-building version: $lineVersion"
			versionArgs=()
			for arg in "${VERSION_INFO_PARTS[@]}"; do
				arg="$(echo -e "${arg}" | sed -e 's/^[[:space:]]*//')"
				versionArgs+=("$arg")
			done

			$funcToCall "${versionArgs[@]}"
		done < "$versionFile"
		return
	fi

	while IFS= read -r VERSION_INFO || [[ -n $VERSION_INFO ]]
	do
		# remove all whitespace before first character
		VERSION_INFO="$(echo -e "${VERSION_INFO}" | sed -e 's/^[[:space:]]*//')"
		# Ignore empty lines and comments
		if [ -z "$VERSION_INFO" ] || [[ $VERSION_INFO = \#* ]] ; then
			continue
		fi

		IFS=',' read -ra VERSION_INFO <<< "$VERSION_INFO"
		versionArgs=()
		for arg in "${VERSION_INFO[@]}"
		do
			# Trim beginning whitespace
			arg="$(echo -e "${arg}" | sed -e 's/^[[:space:]]*//')"
			versionArgs+=("$arg")
		done

		$funcToCall "${versionArgs[@]}"
	done < "$versionFile"
}
