#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh

nodePlatformDir="$REPO_DIR/platforms/node"

builtNodeImage=false
getNode() {
	local version="$1"
	local hostNodeArtifactsDir="$volumeHostDir/nodejs"
	mkdir -p "$hostNodeArtifactsDir"

	if blobExists nodejs nodejs-$version.tar.gz; then
		echo "Node version '$version' already present in blob storage. Skipping it..."
		echo
	else
		echo "Node version '$version' not present in blob storage. Getting it..."
		echo

		if ! $builtNodeImage; then
			docker build \
				-f "$nodePlatformDir/Dockerfile" \
				-t $imageName \
				$REPO_DIR
			builtNodeImage=true
		fi

		docker run \
			-v $hostNodeArtifactsDir:$volumeContainerDir \
			$imageName \
			bash -c "/tmp/scripts/build.sh $version && cp -f /tmp/compressedSdk/* /tmp/sdk"
	fi

	echo "$version" >> "$hostNodeArtifactsDir/versions.txt"
}

echo "Getting Node Sdk..."
echo
buildPlatform "$nodePlatformDir/versions.txt" getNode