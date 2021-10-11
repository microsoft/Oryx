#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh
source $REPO_DIR/build/__phpVersions.sh
debianFlavor=$1
phpPlatformDir="$REPO_DIR/platforms/php"

builtPhpPrereqs=false
buildPhpPrereqsImage() {

    if [ "$debianFlavor" == "focal-scm" ]; then
        # Use default php sdk file name
        phpFlavor="stretch"
    else
        phpFlavor=$debianFlavor
    fi

    if ! $builtPhpPrereqs; then
        echo "Building Php pre-requisites image..."
        echo
        docker build  \
            --build-arg DEBIAN_FLAVOR=$debianFlavor \
            --build-arg PHP_FLAVOR=$phpFlavor \
            -f "$phpPlatformDir/prereqs/Dockerfile" \
            -t "php-build-prereqs" $REPO_DIR
        builtPhpPrereqs=true
    fi
}

buildPhp() {
	local version="$1"
	local sha="$2"
	local gpgKeys="$3"
	local imageName="oryx/php-sdk"
	local targetDir="$volumeHostDir/php"
	local phpSdkFileName=""

	mkdir -p "$targetDir"
	
    if [ "$debianFlavor" == "stretch" ]; then
        # Use default php sdk file name
        phpSdkFileName=php-$version.tar.gz
    else
        phpSdkFileName=php-$debianFlavor-$version.tar.gz
    fi

    if [ "$debianFlavor" == "focal-scm" ]; then
        # Use default php sdk file name
        phpFlavor="stretch"
    else
        phpFlavor=$debianFlavor
    fi

	cp "$phpPlatformDir/defaultVersion.txt" "$targetDir"

	if shouldBuildSdk php $phpSdkFileName || shouldOverwriteSdk || shouldOverwritePhpSdk; then
		if ! $builtPhpPrereqs; then
			buildPhpPrereqsImage
		fi

		echo "Building Php version '$version' in a docker image..."
		echo

		docker build \
			-f "$phpPlatformDir/Dockerfile" \
			--build-arg PHP_VERSION=$version \
			--build-arg PHP_SHA256=$sha \
			--build-arg GPG_KEYS="$gpgKeys" \
			--build-arg DEBIAN_FLAVOR=$debianFlavor \
			--build-arg PHP_FLAVOR=$phpFlavor \
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
	local composerSdkFileName="php-composer-$version.tar.gz"
	mkdir -p "$targetDir"

	cp "$phpPlatformDir/composer/defaultVersion.txt" "$targetDir"

	if [ "$debianFlavor" == "stretch" ]; then
        # Use default php sdk file name
        composerSdkFileName=php-composer-$version.tar.gz
    else
        composerSdkFileName=php-composer-$debianFlavor-$version.tar.gz
    fi

	if shouldBuildSdk php-composer $composerSdkFileName || shouldOverwriteSdk || shouldOverwritePhpComposerSdk; then
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
			--build-arg DEBIAN_FLAVOR=$debianFlavor \
			--build-arg PHP_SHA256="$PHP73_TAR_SHA256" \
			--build-arg GPG_KEYS="$PHP73_KEYS" \
			--build-arg COMPOSER_VERSION="$version" \
			--build-arg COMPOSER_SETUP_SHA384="$COMPOSER_SETUP_SHA384" \
			-t $imageName \
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"
		
		echo "Version=$version" >> "$targetDir/php-composer-$version-metadata.txt"
	fi
}

shouldOverwritePhpSdk() {
	if [ "$OVERWRITE_EXISTING_SDKS_PHP" == "true" ]; then
		return 0
	else
		return 1
	fi
}

shouldOverwritePhpComposerSdk() {
	if [ "$OVERWRITE_EXISTING_SDKS_PHP_COMPOSER" == "true" ]; then
		return 0
	else
		return 1
	fi
}

echo "Building Php..."
echo
buildPlatform "$phpPlatformDir/versionsToBuild.txt" buildPhp

echo
echo "Building Php composer..."
echo
buildPlatform "$phpPlatformDir/composer/versionsToBuild.txt" buildPhpComposer

