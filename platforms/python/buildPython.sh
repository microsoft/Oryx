#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh

pythonPlatformDir="$REPO_DIR/platforms/python"
targetDir="/tmp/compressedSdk/python"
osFlavor=$1
sdkStorageAccountUrl="$2"
mkdir -p "$targetDir"


buildPython() {
	local version="$1"
	local gpgKey="$2"
	local python_sha="$3"
	local imageName="oryx/python"

	local pythonSdkFileName="python-$osFlavor-$version.tar.gz"
	local metadataFile="$targetDir/python-$osFlavor-$version-metadata.txt"
	local sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"

	if shouldBuildSdk python $pythonSdkFileName $sdkStorageAccountUrl || shouldOverwriteSdk || shouldOverwritePlatformSdk python; then
		
		echo "Building Python version '$version' in a docker image..."

		rm -rf /usr/src/python
		mkdir /usr/src/python
		cd /usr/src/python
		OS_FLAVOR=$osFlavor PYTHON_VERSION=$version GPG_KEY=$gpgKey PIP_VERSION=$PIP_VERSION PYTHON_SHA256=$python_sha /tmp/build.sh
		cd $REPO_DIR

		rm -r /opt/python/*

		echo "$sdkVersionMetadataName=$version" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$osFlavor" >> $metadataFile
	fi
}

echo "Building Python..."
echo
buildPlatform "$pythonPlatformDir/versions/$osFlavor/versionsToBuild.txt" buildPython

# Write the default version
cp "$pythonPlatformDir/versions/$osFlavor/defaultVersion.txt" "$targetDir/defaultVersion.$osFlavor.txt"