#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Sinc this file is expected to be 'sourced', we expect the REPO_DIR variable
# to be supplied in the parent script sourcing this file.
source "$REPO_DIR/build/__variables.sh"

volumeHostDir="$ARTIFACTS_DIR/platformSdks"
volumeContainerDir="/tmp/sdk"
imageName="oryx/platformsdk"

blobExists() {
	local containerName="$1"
	local blobName="$2"
	local exitCode=1
	curl -I https://oryxsdksdev.blob.core.windows.net/$containerName/$blobName 2> /tmp/curlError.txt 1> /tmp/curlOut.txt
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