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
	local dockerFile="$3"
	local imageName="oryx/python"

	if shouldBuildSdk python python-$version.tar.gz || shouldOverwriteSdk || shouldOverwritePythonSdk; then
		if ! $builtPythonPrereqs; then
			buildPythonPrereqsImage
		fi
		
		echo "Building Python version '$version' in a docker image..."
		echo

		if [ -z "$dockerFile" ]; then
			# Use common docker file
			dockerFile="$pythonPlatformDir/Dockerfile"
		else
			dockerFile="$pythonPlatformDir/$dockerFile"
		fi
		
		docker build \
			-f "$dockerFile" \
			--build-arg VERSION_TO_BUILD=$version \
			--build-arg GPG_KEYS=$gpgKey \
			--build-arg PIP_VERSION=$PIP_VERSION \
			-t $imageName \
			$REPO_DIR

		getSdkFromImage $imageName "$targetDir"
		
		echo "Version=$version" >> "$targetDir/python-$version-metadata.txt"
	fi
}

shouldOverwritePythonSdk() {
	if [ "$OVERWRITE_EXISTING_SDKS_PYTHON" == "true" ]; then
		return 0
	else
		return 1
	fi
}

echo "Building Python..."
echo
buildPlatform "$pythonPlatformDir/versionsToBuild.txt" buildPython

# Write the default version
cp "$pythonPlatformDir/defaultVersion.txt" $targetDir