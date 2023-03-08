#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh
source $REPO_DIR/build/__rubyVersions.sh

rubyPlatformDir="$REPO_DIR/platforms/ruby"
targetDir="$volumeHostDir/ruby"
debianFlavor=$1
sdkStorageAccountUrl="$2"
mkdir -p "$targetDir"

builtRubyPrereqs=false
buildRubyPrereqsImage() {
	if ! $builtRubyPrereqs; then
		echo "Building Ruby pre-requisites image..."
		echo
		docker build \
			   --build-arg DEBIAN_FLAVOR=$debianFlavor \
			   -f "$rubyPlatformDir/prereqs/Dockerfile" \
			   -t "oryxdevmcr.azurecr.io/private/oryx/ruby-build-prereqs" $REPO_DIR
		builtRubyPrereqs=true
	fi
}

buildRuby() {
	local version="$1"
	local sha="$2"
	local imageName="oryx/ruby"
	local rubySdkFileName=""
	local metadataFile=""
	local sdkVersionMetadataName=""

	if [ "$debianFlavor" == "stretch" ]; then
		# Use default python sdk file name
		rubySdkFileName=ruby-$version.tar.gz
		metadataFile="$targetDir/ruby-$version-metadata.txt"
		# Continue adding the version metadata with the name of Version
		# which is what our legacy CLI will use
		sdkVersionMetadataName="$LEGACY_SDK_VERSION_METADATA_NAME"
		cp "$rubyPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$targetDir/defaultVersion.txt"
	else
		rubySdkFileName=ruby-$debianFlavor-$version.tar.gz
		metadataFile="$targetDir/ruby-$debianFlavor-$version-metadata.txt"
		sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"
	fi 

	if shouldBuildSdk ruby $rubySdkFileName $sdkStorageAccountUrl || shouldOverwriteSdk || shouldOverwritePlatformSdk ruby; then
		if ! $builtRubyPrereqs; then
			buildRubyPrereqsImage
		fi
        
        echo "Building Ruby version '$version' in a docker image..."
		echo

		if [ -z "$dockerFile" ]; then
			# Use common docker file
			dockerFile="$rubyPlatformDir/Dockerfile"
		else
			dockerFile="$rubyPlatformDir/$dockerFile"
		fi

		docker build \
			-f "$rubyPlatformDir/Dockerfile" \
			--build-arg RUBY_VERSION=$version \
            --build-arg RUBY_SHA256=$sha \
			--build-arg GEM_VERSION=$GEM_VERSION \
			-t $imageName \
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"

		echo "$sdkVersionMetadataName=$version" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
	fi
}

echo "Building Ruby..."
echo
buildPlatform "$rubyPlatformDir/versions/$debianFlavor/versionsToBuild.txt" buildRuby

# Write the default version
cp "$rubyPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$targetDir/defaultVersion.$debianFlavor.txt"