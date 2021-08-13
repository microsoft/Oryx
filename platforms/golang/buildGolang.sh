#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh
source $REPO_DIR/build/__golangVersions.sh

golangPlatformDir="$REPO_DIR/platforms/golang"
targetDir="$volumeHostDir/golang"
debianFlavor=$1
mkdir -p "$targetDir"

builtGolangPrereqs=false
buildGolangPrereqsImage() {
	if ! $builtGolangPrereqs; then
		echo "Building Golang pre-requisites image..."
		echo
		docker build \
			   --build-arg DEBIAN_FLAVOR=$debianFlavor \
			   -f "$golangPlatformDir/prereqs/Dockerfile" \
			   -t "golang-build-prereqs" $REPO_DIR
		builtGolangPrereqs=true
	fi
}

buildGolang() {
	local version="$1"
	local sha="$2"
	local imageName="oryx/golang"
	local golangSdkFileName=""

	if [ "$debianFlavor" == "stretch" ]; then
		# Use default python sdk file name
		golangSdkFileName=golang-$version.tar.gz
	else
		golangSdkFileName=golang-$debianFlavor-$version.tar.gz
	fi 

	if shouldBuildSdk golang $golangSdkFileName || shouldOverwriteSdk || shouldOverwriteGolangSdk; then
		if ! $builtGolangPrereqs; then
			buildGolangPrereqsImage
		fi
        
        echo "Building Golang version '$version' in a docker image..."
		echo

		if [ -z "$dockerFile" ]; then
			# Use common docker file
			dockerFile="$golangPlatformDir/Dockerfile"
		else
			dockerFile="$golangPlatformDir/$dockerFile"
		fi

		docker build \
			-f "$golangPlatformDir/Dockerfile" \
			--build-arg GOLANG_VERSION=$version \
            #--build-arg GOLANG_SHA256=$sha \
			#--build-arg GEM_VERSION=$GEM_VERSION \
			-t $imageName \
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"

		echo "Version=$version" >> "$targetDir/golang-$version-metadata.txt"
	fi
}

shouldOverwriteGolangSdk() {
	if [ "$OVERWRITE_EXISTING_SDKS_GOLANG" == "true" ]; then
		return 0
	else
		return 1
	fi
}

echo "Building Golang..."
echo
buildPlatform "$golangPlatformDir/versionsToBuild.txt" buildGolang

# Write the default version
cp "$golangPlatformDir/defaultVersion.txt" $targetDir 