#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------
#
# This script builds some base images that are needed for the build image:
# - Yarn package cache
#

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__phpVersions.sh

IMAGE_DIR_TO_BUILD=$1
BUILD_DIR_PREFIX="$__REPO_DIR/images/build"
ARTIFACTS_FILE="$BASE_IMAGES_ARTIFACTS_FILE_PREFIX/$IMAGE_DIR_TO_BUILD-buildimage-bases.txt"

# Clean artifacts
mkdir -p `dirname $ARTIFACTS_FILE`
> $ARTIFACTS_FILE

volumeHostDir="$ARTIFACTS_DIR/platformSdks"
volumeContainerDir="/tmp/sdk"
mkdir -p "$volumeHostDir"
imageName="oryx/platformsdk"

getSdkFromImage() {
	local imageName="$1"

	echo "Copying sdk file to host directory..."
	echo
	docker run \
		-v $volumeHostDir:$volumeContainerDir \
		$imageName \
		bash -c "cp -f /tmp/compressedSdk/* /tmp/sdk"
}

blobDoesNotExist() {
	local blobName="$1"
	local exitCode=1
	curl -sSf https://oryxsdks.blob.core.windows.net/sdks/$blobName &> /dev/null || exitCode=$?
	if [ "$exitCode" == "0" ]; then
		echo "Blob '$blobName' already exists."
		echo
		return 1
	else
		echo "Blob '$blobName' not found."
		echo
		return 0
	fi
}

buildPythonSdk() {
	local version="$1"
	local gpgKey="$2"
	local pipVersion="$3"
	local dockerFile="$4"
	local imageName="oryx/python-sdk"

	blobDoesNotExist python-$version.tar.gz
	if [ $? -eq 0 ]; then
		echo "Python version '$version' not present in blob storage. Building it in a docker image..."

		if [ -z "$dockerFile" ]; then
			# Use common docker file
			dockerFile="$BUILD_DIR_PREFIX/python/Dockerfile"
		else
			dockerFile="$BUILD_DIR_PREFIX/python/$dockerFile"
		fi

		if [ -z "$pipVersion" ]; then
			# Use default pip version
			pipVersion="$PIP_VERSION"
		fi
		
		docker build \
			-f "$dockerFile" \
			--build-arg VERSION_TO_BUILD=$version \
			--build-arg GPG_KEYS=$gpgKey \
			--build-arg PIP_VERSION=$pipVersion \
			-t $imageName \
			$__REPO_DIR

		getSdkFromImage $imageName
	else
		echo "Python version '$version' already present in blob storage. Skipping building it..."
	fi
}

buildPhpSdk() {
	local version="$1"
	local sha="$2"
	local gpgKeys="$3"
	local dockerFile="$4"
	local imageName="oryx/php-sdk"

	blobDoesNotExist php-$version.tar.gz
	if [ $? -eq 0 ]; then
		echo "Php version '$version' not present in blob storage. Building it in a docker image..."

		if [ -z "$dockerFile" ]; then
			# Use common docker file
			dockerFile="$BUILD_DIR_PREFIX/php/Dockerfile"
		else
			dockerFile="$BUILD_DIR_PREFIX/php/$dockerFile"
		fi
		
		docker build \
			-f "$dockerFile" \
			--build-arg PHP_VERSION=$version \
			--build-arg PHP_SHA256=$sha \
			--build-arg GPG_KEYS=$gpgKeys \
			-t $imageName \
			$__REPO_DIR

		getSdkFromImage $imageName
	else
		echo "Php version '$version' already present in blob storage. Skipping building it..."
	fi
}

getNodeSdk() {
	local version="$1"
	
	blobDoesNotExist nodejs-$version.tar.gz
	if [ $? -eq 0 ]; then
		echo "Node version '$version' not present in blob storage. Getting it..."
		docker build \
			-f "$BUILD_DIR_PREFIX/node/Dockerfile" \
			--build-arg VERSION_TO_BUILD=$version \
			-t $imageName \
			$__REPO_DIR
		
		getSdkFromImage  $imageName
	else
		echo "Node version '$version' already present in blob storage. Skipping it..."
	fi
}

getDotNetCoreSdk() {
	local version="$1"
	local sha="$2"
	local downloadUrl="$3"

	echo "Downloading .NET Core SDK version '$version'..."
	echo

	if [ -z "$downloadUrl" ]; then
		# Use default download url file
		downloadUrl="https://dotnetcli.blob.core.windows.net/dotnet/Sdk/$version/dotnet-sdk-$version-linux-x64.tar.gz"
	fi

	# Follow the format used by all platforms
	downloadedFile="dotnet-$version.tar.gz"
	mkdir -p /tmp/oryx-dotnetInstall
	cd /tmp/oryx-dotnetInstall
	rm -f "$downloadedFile"
	curl -SL $downloadUrl --output "$downloadedFile"
	echo "Verifying archive hash..."
	echo "$sha $downloadedFile" | sha512sum -c -
	cp -f "$downloadedFile" "$volumeHostDir"
	rm -rf /tmp/oryx-dotnetInstall
}

buildSdks() {
	local versionFile="$1"
	local funcToCall="$2"
	while IFS= read -r VERSION_INFO || [[ -n $VERSION_INFO ]]
	do
		# Ignore comments
		if [[ $VERSION_INFO = \#* ]] ; then
			continue
		fi

		IFS=',' read -ra VERSION_INFO <<< "$VERSION_INFO"
		versionArgs=()
		for arg in "${VERSION_INFO[@]}"
		do
			# Trim beginning whitespace
			arg="$(echo -e "${arg}" | sed -e 's/^[[:space:]]*//')"
			versionArgs+=("$arg")
		done

		$funcToCall "${versionArgs[@]}"
	done < "$versionFile"
}

case $IMAGE_DIR_TO_BUILD in
	'dotnet')
		echo "Getting .NET Core SDK..."
		echo
		buildSdks "$BUILD_DIR_PREFIX/dotnet/versions.txt" getDotNetCoreSdk
		;;
	'python')
		echo "Building Python pre-requisites image..."
		echo
		docker build -f $BUILD_DIR_PREFIX/python/prereqs/Dockerfile -t "python-build-prereqs" $__REPO_DIR
		
		buildSdks "$BUILD_DIR_PREFIX/python/versions.txt" buildPythonSdk
		;;
	'php')
		echo "Building Php pre-requisites image..."
		echo
		docker build -f $BUILD_DIR_PREFIX/php/prereqs/Dockerfile -t "php-build-prereqs" $__REPO_DIR

		buildSdks "$BUILD_DIR_PREFIX/php/versions.txt" buildPhpImage
		;;
	'node')
		echo "Getting Node Sdk..."
		echo
		buildSdks "$BUILD_DIR_PREFIX/node/versions.txt" getNodeSdk
		;;            
	'yarn-cache')
		echo "Building Yarn package cache base image"
		echo

		YARN_CACHE_IMAGE_BASE="$ACR_DEV_NAME/public/oryx/build-yarn-cache"
		YARN_CACHE_IMAGE_NAME=$YARN_CACHE_IMAGE_BASE:$IMAGE_TAG

		docker build $BUILD_DIR_PREFIX/yarn-cache -t $YARN_CACHE_IMAGE_NAME
		echo $YARN_CACHE_IMAGE_NAME >> $ARTIFACTS_FILE
		;;
	*) echo "Unknown image directory";;
esac

echo
echo "List of images built (from '$ARTIFACTS_FILE'):"
cat $ARTIFACTS_FILE
echo
