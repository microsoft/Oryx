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
	local exitCode=1
	curl -I $DEV_SDK_STORAGE_BASE_URL/$containerName/$blobName 2> /tmp/curlError.txt 1> /tmp/curlOut.txt
	grep "HTTP/1.1 200 OK" /tmp/curlOut.txt &> /dev/null
	exitCode=$?
	rm -f /tmp/curlOut.txt
	rm -f /tmp/curlError.txt
	if [ $exitCode -eq 0 ]; then
		return 0
	else
		return 1
	fi
}

shouldBuildSdk() {
	local containerName="$1"
	local blobName="$2"

	# return whatever exit code the following returns
	blobExists $containerName $blobName
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
            	return [[ "$OVERWRITE_EXISTING_SDKS_PHP" == "true" ]]; echo "$?"
	    	;;
        "php-composer")
            	return [[ "$OVERWRITE_EXISTING_SDKS_PHP_COMPOSER" == "true" ]]; echo "$?"
            	;;
	"java")
	    	return [[ "$OVERWRITE_EXISTING_SDKS_JAVA" == "true" ]]; echo "$?"
	    	;;
        "maven")
	    	return [[ "$OVERWRITE_EXISTING_SDKS_MAVEN" == "true" ]]; echo "$?"
	    	;;
	"nodejs")
	 	return [[ "$OVERWRITE_EXISTING_SDKS_NODE" == "true" ]]; echo "$?"
	    	;;
        "ruby")
		return [[ "$OVERWRITE_EXISTING_SDKS_RUBY" == "true" ]]; echo "$?"
		;;
	"python")
		return [[ "$OVERWRITE_EXISTING_SDKS_PYTHON" == "true" ]]; echo "$?"
		;;
        "golang")
		return [[ "$OVERWRITE_EXISTING_SDKS_GOLANG" == "true" ]]; echo "$?"
		;;
	"dotnet")
		return [[ "$OVERWRITE_EXISTING_SDKS_DOTNETCORE" == "true" ]]; echo "$?"
		;;
	esac
}

getSdkFromImage() {
	local imageName="$1"
	local hostVolumeDir="$2"

	mkdir -p "$hostVolumeDir"
	echo "Copying sdk file to host directory..."
	echo
	docker run \
		--rm \
		-v $hostVolumeDir:$volumeContainerDir \
		$imageName \
		bash -c "cp -f /tmp/compressedSdk/* /tmp/sdk"
}

buildPlatform() {
	local versionFile="$1"
	local funcToCall="$2"
	while IFS= read -r VERSION_INFO || [[ -n $VERSION_INFO ]]
	do
		# Ignore whitespace and comments
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
