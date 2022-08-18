#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh

nodePlatformDir="$REPO_DIR/platforms/nodejs"
hostNodeArtifactsDir="$volumeHostDir/nodejs"
debianFlavor="$1"
mkdir -p "$hostNodeArtifactsDir"

builtNodeImage=false
getNode() {
	local version="$1"
	
	tarFileName="nodejs-$version.tar.gz"
	metadataFile=""
	sdkVersionMetadataName=""
    
    if [ "$debianFlavor" == "stretch" ]; then
			# Use default sdk file name
			tarFileName=nodejs-$version.tar.gz
			metadataFile="$hostNodeArtifactsDir/nodejs-$version-metadata.txt"
			# Continue adding the version metadata with the name of Version
			# which is what our legacy CLI will use
			sdkVersionMetadataName="$LEGACY_SDK_VERSION_METADATA_NAME"
	else
			tarFileName=nodejs-$debianFlavor-$version.tar.gz
			metadataFile="$hostNodeArtifactsDir/nodejs-$debianFlavor-$version-metadata.txt"
			sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"
	fi

	if shouldBuildSdk nodejs $tarFileName || shouldOverwriteSdk || shouldOverwritePlatformSdk nodejs; then
		echo "Getting Node version '$version'..."
		echo

		if ! $builtNodeImage; then
			docker build \
				--build-arg DEBIAN_FLAVOR=$debianFlavor \
				-f "$nodePlatformDir/Dockerfile" \
				-t $imageName \
				$REPO_DIR
			builtNodeImage=true
		fi

		docker run \
			-v /$hostNodeArtifactsDir:$volumeContainerDir \
			$imageName \
			bash -c "/tmp/scripts/build.sh $version && cp -f /tmp/compressedSdk/* /tmp/sdk"
		
		echo "$sdkVersionMetadataName=$version" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
	fi
}

echo "Getting Node Sdk..."
echo
buildPlatform "$nodePlatformDir/versions/$debianFlavor/versionsToBuild.txt" getNode

# Write the default version
cp "$nodePlatformDir/versions/$debianFlavor/defaultVersion.txt" "$hostNodeArtifactsDir/defaultVersion.$debianFlavor.txt"
