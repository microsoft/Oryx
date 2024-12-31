#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )

source $REPO_DIR/platforms/__common.sh

dotNetPlatformDir="$REPO_DIR/platforms/dotnet"
targetDir="$volumeHostDir/dotnet"
debianFlavor="$1"
sdkStorageAccountUrl="$2"
mkdir -p "$targetDir"

getDotNetCoreSdk() {
	local sdkVersion="$1"
	local sha="$2"
	local downloadUrl="$3"
	local downloadedFile=""
	local metadataFile=""
	local sdkVersionMetadataName=""
	local runtimeVersionMetadataName=""

	if [ "$debianFlavor" == "stretch" ]; then
			# Use default sdk file name
			downloadedFile=dotnet-$sdkVersion.tar.gz
			metadataFile="$targetDir/dotnet-$sdkVersion-metadata.txt"
			# Continue adding the version metadata with the name of Version
			# which is what our legacy CLI will use
			sdkVersionMetadataName="$LEGACY_SDK_VERSION_METADATA_NAME"
			runtimeVersionMetadataName="$LEGACY_DOTNET_RUNTIME_VERSION_METADATA_NAME"
			cp "$dotNetPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$targetDir/defaultVersion.txt"
	else
			downloadedFile=dotnet-$debianFlavor-$sdkVersion.tar.gz
			metadataFile="$targetDir/dotnet-$debianFlavor-$sdkVersion-metadata.txt"
			sdkVersionMetadataName="$SDK_VERSION_METADATA_NAME"
			runtimeVersionMetadataName="$DOTNET_RUNTIME_VERSION_METADATA_NAME"
	fi

	if shouldBuildSdk dotnet $downloadedFile $sdkStorageAccountUrl || shouldOverwriteSdk || shouldOverwritePlatformSdk dotnet; then
		echo "Downloading .NET Core SDK version '$sdkVersion'..."
		echo

		if [ -z "$downloadUrl" ]; then
			# Use default download url file
			downloadUrl="https://builds.dotnet.microsoft.com/dotnet/Sdk/$sdkVersion/dotnet-sdk-$sdkVersion-linux-x64.tar.gz"
		elif  [[ "$downloadUrl" == *"dotnet-private"* ]]; then
			# SAS-token is passed as en env-variable on the Oryx-PlatformBinary-DotNetCore pipeline
			downloadUrl+=$DOTNET_PRIVATE_STORAGE_ACCOUNT_ACCESS_TOKEN
		fi
		

		tempDir="/tmp/oryx-dotnetInstall"
		mkdir -p $tempDir
		cd $tempDir
		rm -f "$downloadedFile"
		curl -SL $downloadUrl --output "$downloadedFile"
		echo "Verifying archive hash..."
		echo "$sha $downloadedFile" | sha512sum -c -
		
		# Find the runtime version
		tar -xzf $downloadedFile -C .
		runtimeVersionDir=$(find "$tempDir/shared/Microsoft.NETCore.App" -mindepth 1 -maxdepth 1 -type d)
		runtimeVersion=$(basename $runtimeVersionDir)
		cp -f "$downloadedFile" "$targetDir"
		rm -rf $tempDir

		echo "$runtimeVersionMetadataName=$runtimeVersion" >> $metadataFile
		echo "$sdkVersionMetadataName=$sdkVersion" >> $metadataFile
		echo "$OS_TYPE_METADATA_NAME=$debianFlavor" >> $metadataFile
	fi
}

echo
echo "Getting .NET Core Sdks..."
echo
buildPlatform "$dotNetPlatformDir/versions/$debianFlavor/versionsToBuild.txt" getDotNetCoreSdk

cp "$dotNetPlatformDir/versions/$debianFlavor/defaultVersion.txt" "$targetDir/defaultVersion.$debianFlavor.txt"
