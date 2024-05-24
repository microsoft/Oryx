#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR="/tmp"

source $REPO_DIR/platforms/__common.sh

nodePlatformDir="$REPO_DIR/platforms/nodejs"
outputDir="/tmp/compressedSdk/nodejs"
debianFlavor="$1"
sdkStorageAccountUrl="$2"

mkdir -p "$outputDir"

getNode() {
	local version="$1"
	
	tarFileName="nodejs-$version.tar.gz"
	metadataFile=""
	sdkVersionMetadataName=""
    
    if [ "$debianFlavor" == "stretch" ]; then
			# Use default sdk file name
			tarFileName=nodejs-$version.tar.gz
			metadataFile="$outputDir/nodejs-$version-metadata.txt"
			# Continue adding the version metadata with the name of Version
			# which is what our legacy CLI will use
			sdkVersionMetadataName="$LEGACY_SDK_VERSION_METADATA_NAME"
			cp "$nodePlatformDir/versions/$debianFlavor/defaultVersion.txt" "$outputDir/defaultVersion.txt"
	else
			tarFileName=nodejs-$debianFlavor-$version.tar.gz
			metadataFile="$outputDir/nodejs-$debianFlavor-$version-metadata.txt"
			sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"
	fi

	if shouldBuildSdk nodejs $tarFileName $sdkStorageAccountUrl || shouldOverwriteSdk || shouldOverwritePlatformSdk nodejs; then
		echo "Getting Node version '$version'..."
		echo

		/tmp/scripts/build.sh $version
		
		echo "$sdkVersionMetadataName=$version" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
	fi
}

echo "Getting Node Sdk..."
echo
buildPlatform "$nodePlatformDir/versions/$debianFlavor/versionsToBuild.txt" getNode

# Write the default version
cp "$nodePlatformDir/versions/$debianFlavor/defaultVersion.txt" "$outputDir/defaultVersion.$debianFlavor.txt"
