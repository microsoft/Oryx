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
mkdir -p "$targetDir"

builtRubyPrereqs=false
buildRubyPrereqsImage() {
	if ! $builtRubyPrereqs; then
		echo "Building Ruby pre-requisites image..."
		echo
		docker build -f "$rubyPlatformDir/prereqs/Dockerfile" -t "ruby-build-prereqs" $REPO_DIR
		builtRubyPrereqs=true
	fi
}

buildRuby() {
	local version="$1"
	local dockerFile="$2"
	local imageName="oryx/ruby"

	if shouldBuildSdk ruby ruby-$version.tar.gz || shouldOverwriteSdk || shouldOverwriteRubySdk; then
		echo "Building Ruby version '$version' in a docker image..."
		echo

		if [ -z "$dockerFile" ]; then
			# Use common docker file
			dockerFile="$rubyPlatformDir/Dockerfile"
		else
			dockerFile="$rubyPlatformDir/$dockerFile"
		fi

		docker build \
			-f "$dockerFile" \
			--build-arg VERSION_TO_BUILD=$version \
			--build-arg GEM_VERSION=$GEM_VERSION \
			-t $imageName \
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"

		echo "Version=$version" >> "$targetDir/ruby-$version-metadata.txt"
	fi
}

shouldOverwriteRubySdk() {
	if [ "$OVERWRITE_EXISTING_SDKS_RUBY" == "true" ]; then
		return 0
	else
		return 1
	fi
}

echo "Building Ruby..."
echo
buildPlatform "$rubyPlatformDir/versionsToBuild.txt" buildRuby

# Write the default version
cp "$rubyPlatformDir/defaultVersion.txt" $targetDir 