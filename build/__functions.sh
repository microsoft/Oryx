#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Folder structure is used to decide the tag name
# For example, if a Dockerfile is located at "images/runtime/node/10.1.0/Dockerfile",
# then the tag name would be 'node:10.1.0' (i.e. the path between 'runtime' and 'Dockerfile' segments)
# Additionally, if a os type such as debian-bullseye is passed in, we append the os type to the tag as well like
# node:10.1.0-debian-bullseye
function getTagName()
{
	if [ ! -d $1 ]
	then
		echo "Directory '$1' does not exist."
		return 1
	fi

	osTypeSuffix=""
	if [ ! -z $2 ]
	then
		osTypeSuffix="-$2"
	fi

	local replacedPath="$RUNTIME_IMAGES_SRC_DIR/"
	echo "Runtime Image Source Directory: "$RUNTIME_IMAGES_SRC_DIR
	local remainderPath="${1//$replacedPath/}"
	tagNameFile="$RUNTIME_IMAGES_SRC_DIR/$remainderPath/tag.txt"
	
	if [ -f "$tagNameFile" ]
	then
		getTagName_result=$(cat $tagNameFile)
		echo "tagname for "$replacedPath" is :"$getTagName_result
		return 0
	fi

	local slashChar="/"
	getTagName_result="${remainderPath//$slashChar/":"}$osTypeSuffix"
	return 0
}

function dockerCleanupIfRequested()
{
	if [ "$DOCKER_SYSTEM_PRUNE" == "true" ]
	then
		echo "Running 'docker system prune -f'"
		docker system prune -f
	else
		echo "Skipping 'docker system prune -f'"
	fi
}

function execAllGenerateDockerfiles()
{
	runtimeImagesSourceDir="$1"
	runtimeGenerateDockerFileScriptName="$2"
	runtimeImageDebianFlavor=$3

	echo "runtime image type '$3'"
	echo "runtimeGenerateDockerFileScriptName '$2'"

	generateDockerfiles=$(find $runtimeImagesSourceDir -type f -name $runtimeGenerateDockerFileScriptName)
	if [ -z "$generateDockerfiles" ]
	then
		echo "Couldn't find any '$runtimeGenerateDockerFileScriptName' under '$runtimeImagesSourceDir' and its sub-directories."
	fi

	for generateDockerFile in $generateDockerfiles; do
		echo
		echo "Executing '$generateDockerFile'..."
		echo
		eval "$(echo "$generateDockerFile $runtimeImageDebianFlavor")"
	done
}

function showDockerImageSizes()
{
	docker system df -v
}

function shouldStageRuntimeVersion()
{
	platformName="$1"
	platformRuntimeVersion="$2"

	declare -A PLATFORM_RUNTIME_VERSIONS=(
		['dotnet']="${DOTNETCORE_STAGING_RUNTIME_VERSIONS[*]}"
		['dotnetcore']="${DOTNETCORE_STAGING_RUNTIME_VERSIONS[*]}"
		['python']="${PYTHON_STAGING_RUNTIME_VERSIONS[*]}"
		['node']="${NODE_STAGING_RUNTIME_VERSIONS[*]}"
		['java']="${JAVA_STAGING_RUNTIME_VERSIONS[*]}"
		['php']="${PHP_STAGING_RUNTIME_VERSIONS[*]}"
		['hugo']="${HUGO_STAGING_RUNTIME_VERSIONS[*]}"
		['golang']="${GOLANG_STAGING_RUNTIME_VERSIONS[*]}"
	)

	if [[ " ${PLATFORM_RUNTIME_VERSIONS[$platformName]} " =~ " ${platformRuntimeVersion} " ]]; then
		return 0
	else
		echo "Platform '$platformName' does not support staging."
		return 1
	fi
}

function retrieveSastokenFromKeyVault()
{	
	set +x
	sdkStorageAccountUrl="$1"

	if [ $sdkStorageAccountUrl == $PRIVATE_STAGING_SDK_STORAGE_BASE_URL ] && [ -z "$ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN" ]; then
	
		echo "Retrieving token from the Keyvault and setting it to the environment variable 'ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN'"
		stagingPrivateStorageSasToken=$(az keyvault secret show --name "ORYX-SDK-STAGING-PRIVATE-SAS-TOKEN" --vault-name "oryx" --query value -o tsv)
	
    	export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN=$stagingPrivateStorageSasToken
	fi
	set -x
}
