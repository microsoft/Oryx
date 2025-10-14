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
osFlavor="$1"
sdkStorageAccountUrl="$2"

mkdir -p "$outputDir"

getNode() {
	local version="$1"

	tarFileName=nodejs-$osFlavor-$version.tar.gz
	metadataFile="$outputDir/nodejs-$osFlavor-$version-metadata.txt"
	sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"

	if shouldBuildSdk nodejs $tarFileName $sdkStorageAccountUrl || shouldOverwriteSdk || shouldOverwritePlatformSdk nodejs; then
		echo "Getting Node version '$version'..."
		echo

		/tmp/scripts/build.sh $version
		
		echo "$sdkVersionMetadataName=$version" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$osFlavor" >> $metadataFile
	fi
}

echo "Getting Node Sdk..."
echo
buildPlatform "$nodePlatformDir/versions/$osFlavor/versionsToBuild.txt" getNode

# Write the default version
cp "$nodePlatformDir/versions/$osFlavor/defaultVersion.txt" "$outputDir/defaultVersion.$osFlavor.txt"
