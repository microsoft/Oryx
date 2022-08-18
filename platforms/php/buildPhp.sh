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
	if ! $builtPhpPrereqs; then
		echo "Building Php pre-requisites image..."
		echo
		docker build  \
			--build-arg DEBIAN_FLAVOR=$debianFlavor \
			-f "$phpPlatformDir/prereqs/Dockerfile" \
			-t "oryxdevmcr.azurecr.io/private/oryx/php-build-prereqs" $REPO_DIR
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
	local metadataFile=""
	local sdkVersionMetadataName=""

	mkdir -p "$targetDir"
	
	if [ "$debianFlavor" == "stretch" ]; then
		# Use default php sdk file name
		phpSdkFileName=php-$version.tar.gz
		metadataFile="$targetDir/php-$version-metadata.txt"
		# Continue adding the version metadata with the name of Version
		# which is what our legacy CLI will use
		sdkVersionMetadataName="$LEGACY_SDK_VERSION_METADATA_NAME"
	else
		phpSdkFileName=php-$debianFlavor-$version.tar.gz
		metadataFile="$targetDir/php-$debianFlavor-$version-metadata.txt"
		sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"
	fi

	cp "$phpPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$targetDir/defaultVersion.$debianFlavor.txt"

	if shouldBuildSdk php $phpSdkFileName || shouldOverwriteSdk || shouldOverwritePlatformSdk php; then
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
			-t $imageName \
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"
		
		echo "$sdkVersionMetadataName=$version" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
	fi
}

buildPhpComposer() {
	local version="$1"
	local sha="$2"
	local imageName="oryx/php-composer-sdk"
	local targetDir="$volumeHostDir/php-composer"
	local composerSdkFileName="php-composer-$version.tar.gz"
	local metadataFile=""
	local sdkVersionMetadataName=""
	mkdir -p "$targetDir"

	cp "$phpPlatformDir/composer/versions/$debianFlavor/defaultVersion.txt" "$targetDir/defaultVersion.$debianFlavor.txt"

	if [ "$debianFlavor" == "stretch" ]; then
		# Use default php sdk file name
		composerSdkFileName=php-composer-$version.tar.gz
		metadataFile="$targetDir/php-composer-$version-metadata.txt"
		# Continue adding the version metadata with the name of Version
		# which is what our legacy CLI will use
		sdkVersionMetadataName="$LEGACY_SDK_VERSION_METADATA_NAME"
	else
		composerSdkFileName=php-composer-$debianFlavor-$version.tar.gz
		metadataFile="$targetDir/php-composer-$debianFlavor-$version-metadata.txt"
		sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"
	fi

	if shouldBuildSdk php-composer $composerSdkFileName || shouldOverwriteSdk || shouldOverwritePlatformSdk php-composer; then
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
		
		echo "$sdkVersionMetadataName=$version" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
	fi
}

echo "Building Php..."
echo
buildPlatform "$phpPlatformDir/versions/$debianFlavor/versionsToBuild.txt" buildPhp

echo
echo "Building Php composer..."
echo
buildPlatform "$phpPlatformDir/composer/versions/$debianFlavor/versionsToBuild.txt" buildPhpComposer

