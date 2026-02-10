#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh
# source $REPO_DIR/build/__phpVersions.sh
osFlavor=$1
sdkStorageAccountUrl="$2"
phpType=$3
phpPlatformDir="$REPO_DIR/platforms/php"


buildPhp() {
	local version="$1"
	local sha="$2"
	local gpgKeys="$3"
	local imageName="oryx/php-sdk"
	local targetDir="/tmp/compressedSdk/php"
	local phpSdkFileName=""
	local metadataFile=""
	local sdkVersionMetadataName=""

	mkdir -p "$targetDir"

	phpSdkFileName=php-$osFlavor-$version.tar.gz
	metadataFile="$targetDir/php-$osFlavor-$version-metadata.txt"
	sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"

	cp "$phpPlatformDir/versions/$osFlavor/defaultVersion.txt" "$targetDir/defaultVersion.$osFlavor.txt"

	if shouldBuildSdk php $phpSdkFileName $sdkStorageAccountUrl || shouldOverwriteSdk || shouldOverwritePlatformSdk php; then

		echo "Building Php version '$version' in a docker image..."
		echo

		PHP_VERSION=$version GPG_KEYS=$gpgKeys PHP_SHA256=$sha /php/build.sh

		rm -r /opt/php/*

		# docker build \
		# 	-f "$phpPlatformDir/Dockerfile" \
		# 	--build-arg PHP_VERSION=$version \
		# 	--build-arg PHP_SHA256=$sha \
		# 	--build-arg GPG_KEYS="$gpgKeys" \
		# 	-t $imageName \
		# 	$REPO_DIR
		
		echo "$sdkVersionMetadataName=$version" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$osFlavor" >> $metadataFile
	fi
}

buildPhpComposer() {
	local version="$1"
	local sha="$2"
	local imageName="oryx/php-composer-sdk"
	local targetDir="/tmp/compressedSdk/php-composer"
	local composerSdkFileName="php-composer-$version.tar.gz"
	local metadataFile=""
	local sdkVersionMetadataName=""
	mkdir -p "$targetDir"

	cp "$phpPlatformDir/composer/versions/$osFlavor/defaultVersion.txt" "$targetDir/defaultVersion.$osFlavor.txt"

	composerSdkFileName=php-composer-$osFlavor-$version.tar.gz
	metadataFile="$targetDir/php-composer-$osFlavor-$version-metadata.txt"
	sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"

	if shouldBuildSdk php-composer $composerSdkFileName $sdkStorageAccountUrl || shouldOverwriteSdk || shouldOverwritePlatformSdk php-composer; then

		echo "Php composer version '$version' not present in blob storage. Building it in a docker image..."
		echo

		PHP_VERSION=$PHP_VERSION GPG_KEYS=$GPG_KEYS PHP_SHA256=$PHP_SHA256 /php/build.sh

		# Installing PHP composer requires having PHP installed in an first image first, so we try installing
		# a version here.
		# docker build \
		# 	-f "$phpPlatformDir/composer/Dockerfile" \
		# 	--build-arg PHP_VERSION="$PHP81_VERSION" \
		# 	--build-arg OS_FLAVOR=$osFlavor \
		# 	--build-arg PHP_SHA256="$PHP81_TAR_SHA256" \
		# 	--build-arg GPG_KEYS="$PHP81_KEYS" \
		# 	--build-arg COMPOSER_VERSION="$version" \
		# 	--build-arg COMPOSER_SETUP_SHA384="$COMPOSER_SETUP_SHA384" \
		# 	-t $imageName \
		# 	$REPO_DIR

		set -ex
		composerDir="/opt/php-composer/$version"
		mkdir -p "$composerDir"
		export phpbin="/opt/php/$PHP_VERSION/bin/php" 
		$phpbin /tmp/platforms/php/composer-setup.php --version=$version --install-dir="$composerDir" 
		compressedSdkDir="/tmp/compressedSdk/php-composer"
		mkdir -p "$compressedSdkDir"
		cd "$composerDir"
		echo 'os flavor is: $osFlavor' 
		composerSdkFile="php-composer-$osFlavor-$version.tar.gz"
		tar -zcf "$compressedSdkDir/$composerSdkFile" .
        # Removing php sdk that is generated as part of composer build
		rm -r /tmp/compressedSdk/php/php-$osFlavor-$PHP_VERSION.tar.gz
		rm -r ./*
		rm -r /opt/php/*

		echo "$sdkVersionMetadataName=$version" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$osFlavor" >> $metadataFile
	fi
}

if [ "$phpType" == "php" ]; then
	echo "Building Php..."
	echo
	buildPlatform "$phpPlatformDir/versions/$osFlavor/versionsToBuild.txt" buildPhp
else
	echo "Building Php composer..."
	echo
	buildPlatform "$phpPlatformDir/composer/versions/$osFlavor/versionsToBuild.txt" buildPhpComposer
fi
