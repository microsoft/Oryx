#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r IMAGE_NAME_PLACEHOLDER="%DOTNETCORE_BASE_IMAGE%"

function generateFiles()
{
	local versionsFile="$1"
	local dockerTemplate="$2"

	# Example line:
	# 1.0, microsoft/dotnet:1.0.13-runtime
	while IFS= read -r VERSION_BASEIMAGE_TUPLE_LINE || [[ -n $VERSION_BASEIMAGE_TUPLE_LINE ]]
	do
		IFS=',' read -ra VERSION_BASEIMAGE_TUPLE <<< "$VERSION_BASEIMAGE_TUPLE_LINE"
		VERSION_DIRECTORY="${VERSION_BASEIMAGE_TUPLE[0]}"
		DOTNET_IMAGE_NAME="${VERSION_BASEIMAGE_TUPLE[1]}"
		# Trim beginning whitespace
		DOTNET_IMAGE_NAME="$(echo -e "${DOTNET_IMAGE_NAME}" | sed -e 's/^[[:space:]]*//')"
		echo "Generating Dockerfile for image '$DOTNET_IMAGE_NAME' in directory '$VERSION_DIRECTORY'..."
		
		mkdir -p "$DIR/$VERSION_DIRECTORY/"
		TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/Dockerfile"
		cp "$dockerTemplate" "$TARGET_DOCKERFILE"

		# Replace placeholders
		sed -i "s|$IMAGE_NAME_PLACEHOLDER|$DOTNET_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
	done < "$versionsFile"
}

generateFiles "$DIR/dotnetCoreVersionsWithCurlUpdate.txt" "$DIR/DockerfileWithCurlUpdate.template"
generateFiles "$DIR/dotnetCoreVersions.txt" "$DIR/Dockerfile.template"