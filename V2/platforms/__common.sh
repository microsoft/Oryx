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
	$blobName="$1"
	if [[ "$blobName" == "defaultVersion"* ]]; then
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
