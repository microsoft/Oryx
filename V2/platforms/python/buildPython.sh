#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh
source $REPO_DIR/build/__pythonVersions.sh

pythonPlatformDir="$REPO_DIR/platforms/python"
targetDir="/tmp/compressedSdk/python"
debianFlavor=$1
sdkStorageAccountUrl="$2"
mkdir -p "$targetDir"


buildPython() {
	local version="$1"
	local gpgKey="$2"
	local dockerFile="$3"
	local imageName="oryx/python"
	local pythonSdkFileName=""
	local metadataFile=""
	local sdkVersionMetadataName=""

	if [ "$debianFlavor" == "stretch" ]; then
			# Use default python sdk file name
			pythonSdkFileName=python-$version.tar.gz
			metadataFile="$targetDir/python-$version-metadata.txt"
			# Continue adding the version metadata with the name of Version
			# which is what our legacy CLI will use
			sdkVersionMetadataName="$LEGACY_SDK_VERSION_METADATA_NAME"
			cp "$pythonPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$targetDir/defaultVersion.txt"
	else
			pythonSdkFileName=python-$debianFlavor-$version.tar.gz
			metadataFile="$targetDir/python-$debianFlavor-$version-metadata.txt"
			sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"
	fi

	if shouldBuildSdk python $pythonSdkFileName $sdkStorageAccountUrl || shouldOverwriteSdk || shouldOverwritePlatformSdk python; then
		
		echo "Building Python version '$version' in a docker image..."
		echo

        echo "dockerfile is : $pythonPlatformDir/$dockerFile"
		if [ -z "$dockerFile" ]; then
			# Use common docker file
			dockerFile="$pythonPlatformDir/Dockerfile"
		else
			dockerFile="$pythonPlatformDir/$dockerFile"
		fi
		
		cat $dockerFile

		DEBIAN_FLAVOR=$debianFlavor PYTHON_VERSION=$version GPG_KEYS=$gpgKey PIP_VERSION=$PIP_VERSION /tmp/build.sh

		rm -r /opt/python/*

		echo "$sdkVersionMetadataName=$version" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
	fi
}

echo "Building Python..."
echo
buildPlatform "$pythonPlatformDir/versions/$debianFlavor/versionsToBuild.txt" buildPython

# Write the default version
cp "$pythonPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$targetDir/defaultVersion.$debianFlavor.txt"