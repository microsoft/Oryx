#!/bin/bash

set -e

declare -r DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
declare -r VERSIONS_FILE="$DIR/dotnetCoreVersions.txt"
declare -r DOCKERFILE_TEMPLATE="$DIR/Dockerfile.template"
declare -r IMAGE_NAME_PLACEHOLDER="%DOTNETCORE_BASE_IMAGE%"
declare -r ALPINE_OR_STRETCH_PLACEHOLDER="%DOTNETCORE_BASE_IMAGE_ALPINE_OR_STRETCH%"

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

	GO_IMAGE_TYPE="stretch"
	# Figure out if the final image is Alpine based
	if [[ $DOTNET_IMAGE_NAME == *"alpine"* ]]; then
		GO_IMAGE_TYPE="alpine"
	fi
	
	mkdir -p "$DIR/$VERSION_DIRECTORY/"
	TARGET_DOCKERFILE="$DIR/$VERSION_DIRECTORY/Dockerfile"
	cp "$DOCKERFILE_TEMPLATE" "$TARGET_DOCKERFILE"

	# Replace placeholders
	sed -i "s|$IMAGE_NAME_PLACEHOLDER|$DOTNET_IMAGE_NAME|g" "$TARGET_DOCKERFILE"
	sed -i "s|$ALPINE_OR_STRETCH_PLACEHOLDER|$GO_IMAGE_TYPE|g" "$TARGET_DOCKERFILE"

done < "$VERSIONS_FILE"