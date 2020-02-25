#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh
source $REPO_DIR/build/__pythonVersions.sh

pythonPlatformDir="$REPO_DIR/platforms/python"
targetDir="$volumeHostDir/python"
mkdir -p "$targetDir"

builtPythonPrereqs=false
buildPythonPrereqsImage() {
	if ! $builtPythonPrereqs; then
		echo "Building Python pre-requisites image..."
		echo
		docker build -f "$pythonPlatformDir/prereqs/Dockerfile" -t "python-build-prereqs" $REPO_DIR
		builtPythonPrereqs=true
	fi
}

buildPython() {
	local version="$1"
	local gpgKey="$2"
	local pipVersion="$3"
	local dockerFile="$4"
	local imageName="oryx/python"

	if blobExists python python-$version.tar.gz; then
		echo "Python version '$version' already present in blob storage. Skipping building it..."
		echo
	else
		if ! $builtPythonPrereqs; then
			buildPythonPrereqsImage
		fi
		
		echo "Python version '$version' not present in blob storage. Building it in a docker image..."
		echo

		if [ -z "$dockerFile" ]; then
			# Use common docker file
			dockerFile="$pythonPlatformDir/Dockerfile"
		else
			dockerFile="$pythonPlatformDir/$dockerFile"
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
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"
		
		echo "version=$version" >> "$targetDir/python-$version-metadata.txt"
	fi
}

echo "Building Python..."
echo
buildPlatform "$pythonPlatformDir/versionsToBuild.txt" buildPython

# Write the default version
defaultVersion=$(getDefaultVersion "$pythonPlatformDir/defaultVersion.txt")
echo "$defaultVersion" > "$targetDir/defaultVersion.txt"