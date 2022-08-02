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

	if [ "$debianFlavor" == "stretch" ]; then
		# Use default python sdk file name
		rubySdkFileName=ruby-$version.tar.gz
	else
		rubySdkFileName=ruby-$debianFlavor-$version.tar.gz
	fi 

	if shouldBuildSdk ruby $rubySdkFileName || shouldOverwriteSdk || shouldOverwritePlatformSdk ruby; then
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

		echo "Version=$version" >> "$targetDir/ruby-$version-metadata.txt"
	fi
}

echo "Building Ruby..."
echo
buildPlatform "$rubyPlatformDir/versionsToBuild.txt" buildRuby

# Write the default version
cp "$rubyPlatformDir/defaultVersion.txt" $targetDir 