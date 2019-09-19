#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh

phpPlatformDir="$REPO_DIR/platforms/php"

builtPhpPrereqs=false
buildPhpPrereqsImage() {
	if ! $builtPhpPrereqs; then
		echo "Building Php pre-requisites image..."
		echo
		docker build -f "$phpPlatformDir/prereqs/Dockerfile" -t "php-build-prereqs" $REPO_DIR
		builtPhpPrereqs=true
	fi
}

buildPhp() {
	local version="$1"
	local sha="$2"
	local gpgKeys="$3"
	local dockerFile="$4"
	local imageName="oryx/php-sdk"
	local targetDir="$volumeHostDir/php"
	mkdir -p "$targetDir"

	if blobExists php php-$version.tar.gz; then
		echo "Php version '$version' already present in blob storage. Skipping building it..."
		echo
	else
		buildPhpPrereqsImage
		
		echo "Php version '$version' not present in blob storage. Building it in a docker image..."
		echo

		if [ -z "$dockerFile" ]; then
			# Use common docker file
			dockerFile="$phpPlatformDir/Dockerfile"
		else
			dockerFile="$phpPlatformDir/$dockerFile"
		fi
		
		docker build \
			-f "$dockerFile" \
			--build-arg PHP_VERSION=$version \
			--build-arg PHP_SHA256=$sha \
			--build-arg GPG_KEYS="$gpgKeys" \
			-t $imageName \
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"
	fi

	echo "$version" >> "$targetDir/versions.txt"
}

echo "Building Php..."
echo
buildPlatform "$phpPlatformDir/versions.txt" buildPhp