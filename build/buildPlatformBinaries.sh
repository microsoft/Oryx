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
source $REPO_DIR/build/__pythonVersions.sh

PLATFORM_TO_BUILD=$1
BUILD_DIR_PREFIX="$__REPO_DIR/images/build"

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

blobExists() {
	local blobName="$1"
	local exitCode=1
	curl -I https://oryxsdks.blob.core.windows.net/sdks/$blobName 2> /tmp/curlError.txt 1> /tmp/curlOut.txt
	grep "HTTP/1.1 200 OK" /tmp/curlOut.txt &> /dev/null
	if [ $? -eq 0 ]; then
		return 0
	else
		return 1
	fi
}

builtPythonPrereqs=false
buildPythonPrereqsImage() {
	if ! $builtPythonPrereqs; then
		echo "Building Python pre-requisites image..."
		echo
		docker build -f $BUILD_DIR_PREFIX/python/prereqs/Dockerfile -t "python-build-prereqs" $__REPO_DIR
		builtPythonPrereqs=true
	fi
}

buildPython() {
	local version="$1"
	local gpgKey="$2"
	local pipVersion="$3"
	local dockerFile="$4"
	local imageName="oryx/python"

	if blobExists python-$version.tar.gz; then
		echo "Python version '$version' already present in blob storage. Skipping building it..."
		echo
	else
		buildPythonPrereqsImage
		
		echo "Python version '$version' not present in blob storage. Building it in a docker image..."
		echo

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
	fi
}

builtPhpPrereqs=false
buildPhpPrereqsImage() {
	if ! $builtPhpPrereqs; then
		echo "Building Php pre-requisites image..."
		echo
		docker build -f $BUILD_DIR_PREFIX/php/prereqs/Dockerfile -t "php-build-prereqs" $__REPO_DIR
		builtPhpPrereqs=true
	fi
}

buildPhp() {
	local version="$1"
	local sha="$2"
	local gpgKeys="$3"
	local dockerFile="$4"
	local imageName="oryx/php-sdk"

	if blobExists php-$version.tar.gz; then
		echo "Php version '$version' already present in blob storage. Skipping building it..."
		echo
	else
		buildPhpPrereqsImage
		
		echo "Php version '$version' not present in blob storage. Building it in a docker image..."
		echo

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
			--build-arg GPG_KEYS="$gpgKeys" \
			-t $imageName \
			$__REPO_DIR

		getSdkFromImage $imageName
	fi
}

builtNodeImage=false
getNode() {
	local version="$1"
	
	if blobExists nodejs-$version.tar.gz; then
		echo "Node version '$version' already present in blob storage. Skipping it..."
		echo
	else
		echo "Node version '$version' not present in blob storage. Getting it..."
		echo

		if ! $builtNodeImage; then
			docker build \
				-f "$BUILD_DIR_PREFIX/node/Dockerfile" \
				-t $imageName \
				$__REPO_DIR
			builtNodeImage=true
		fi

		docker run \
			-v $volumeHostDir:$volumeContainerDir \
			$imageName \
			bash -c "/tmp/scripts/build.sh $version && cp -f /tmp/compressedSdk/* /tmp/sdk"
	fi
}

getDotNetCoreSdk() {
	local version="$1"
	local sha="$2"
	local downloadUrl="$3"

	if blobExists dotnet-$version.tar.gz; then
		echo ".NET Core version '$version' already present in blob storage. Skipping it..."
		echo
	else
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
	fi
}

buildSdks() {
	local versionFile="$1"
	local funcToCall="$2"
	while IFS= read -r VERSION_INFO || [[ -n $VERSION_INFO ]]
	do
		# Ignore whitespace and comments
		if [ -z "$VERSION_INFO" ] || [[ $VERSION_INFO = \#* ]] ; then
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

case $PLATFORM_TO_BUILD in
	'dotnet')
		echo "Getting .NET Core SDK..."
		echo
		buildSdks "$BUILD_DIR_PREFIX/dotnet/versions.txt" getDotNetCoreSdk
		;;
	'python')
		echo "Building Python..."
		echo
		buildSdks "$BUILD_DIR_PREFIX/python/versions.txt" buildPython
		;;
	'php')
		echo "Building Php..."
		echo
		buildSdks "$BUILD_DIR_PREFIX/php/versions.txt" buildPhp
		;;
	'node')
		echo "Getting Node Sdk..."
		echo
		buildSdks "$BUILD_DIR_PREFIX/node/versions.txt" getNode
		;;            
	*) echo "Unknown image directory";;
esac
