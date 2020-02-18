#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh
source $REPO_DIR/build/__phpVersions.sh

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
	local imageName="oryx/php-sdk"
	local targetDir="$volumeHostDir/php"
	mkdir -p "$targetDir"

	if blobExists php php-$version.tar.gz; then
		echo "Php version '$version' already present in blob storage. Skipping building it..."
		echo
	else
		if ! $builtPhpPrereqs; then
			buildPhpPrereqsImage
		fi

		echo "Php version '$version' not present in blob storage. Building it in a docker image..."
		echo

		docker build \
			-f "$phpPlatformDir/Dockerfile" \
			--build-arg PHP_VERSION=$version \
			--build-arg PHP_SHA256=$sha \
			--build-arg GPG_KEYS="$gpgKeys" \
			-t $imageName \
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"
		
		echo "version=$version" >> "$targetDir/php-$version-metadata.txt"
	fi
}

buildPhpComposer() {
	local version="$1"
	local sha="$2"
	local imageName="oryx/php-composer-sdk"
	local targetDir="$volumeHostDir/php-composer"
	mkdir -p "$targetDir"

	if blobExists php-composer php-composer-$version.tar.gz; then
		echo "Php composer version '$version' already present in blob storage. Skipping building it..."
		echo
	else
		if ! $builtPhpPrereqs; then
			buildPhpPrereqsImage
		fi

		echo "Php composer version '$version' not present in blob storage. Building it in a docker image..."
		echo

		# Installing PHP composer requires having PHP installed in an first image first, so we try installing
		# a version here.
		docker build \
			-f "$phpPlatformDir/composer/Dockerfile" \
			--build-arg PHP_VERSION="$PHP73_VERSION" \
			--build-arg PHP_SHA256="$PHP73_TAR_SHA256" \
			--build-arg GPG_KEYS="$PHP73_KEYS" \
			--build-arg COMPOSER_VERSION="$version" \
			--build-arg COMPOSER_SHA384="$sha" \
			-t $imageName \
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"
		
		echo "version=$version" >> "$targetDir/php-composer-$version-metadata.txt"
	fi
}

echo "Building Php..."
echo
buildPlatform "$phpPlatformDir/versionsToBuild.txt" buildPhp

echo
echo "Building Php composer..."
echo
buildPlatform "$phpPlatformDir/composer/versionsToBuild.txt" buildPhpComposer