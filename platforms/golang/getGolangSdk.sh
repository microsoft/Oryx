#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh

golangPlatformDir="$REPO_DIR/platforms/golang"
targetDir="$volumeHostDir/golang"
debianFlavor="$1"
mkdir -p "$targetDir"

getGolangSdk() {
	local sdkVersion="$1"
	local sha="$2"
	local downloadUrl="$3"
	local downloadedFile=""
	local metadataFile=""
	local golangSdkSourceFileName=go$sdkVersion.linux-amd64.tar.gz

	if [ "$debianFlavor" == "stretch" ]; then
		# Use default sdk file name
		downloadedFile=golang-$sdkVersion.tar.gz
		metadataFile="$targetDir/golang-$sdkVersion-metadata.txt"
	else
		downloadedFile=golang-$debianFlavor-$sdkVersion.tar.gz
		metadataFile="$targetDir/golang-$debianFlavor-$sdkVersion-metadata.txt"
	fi

	if shouldBuildSdk golang $downloadedFile || shouldOverwriteSdk || shouldOverwritePlatformSdk golang; then
		echo "Downloading golang SDK version '$sdkVersion'..."
		echo

		if [ -z "$downloadUrl" ]; then
			# Use default download url file
			downloadUrl="https://golang.org/dl/$golangSdkSourceFileName"
		fi

		tempDir="/tmp/oryx-golangInstall"
		mkdir -p $tempDir
		cd $tempDir
		rm -f "$downloadedFile"
		curl -SL $downloadUrl --output "$downloadedFile"
		echo "Verifying archive hash..."
		echo "$sha $downloadedFile" | sha256sum -c -
		# Untar the golang sdk tar
		tar -xzf $downloadedFile -C .
		cp -f "$downloadedFile" "$targetDir"
		rm -rf $tempDir

		echo "Version=$sdkVersion" >> $metadataFile
	fi
}

echo
echo "Getting golang Sdks..."
echo
buildPlatform "$golangPlatformDir/versions/$debianFlavor/versionsToBuild.txt" getGolangSdk

cp "$golangPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$targetDir/defaultVersion.$debianFlavor.txt"
