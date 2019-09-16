#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

if [ -n "$BUILD_BUILDIMAGES_USING_NOCACHE" ]; then
	echo
	echo "Building buildpack runner image with NO cache..."
	noCacheFlag='--no-cache'
else
	echo
	echo "Building buildpack runner image..."
fi

# Build an image that runs `pack`
echo "-> Building pack runner image: $ACR_PACK_IMAGE_REPO"
echo
cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"
docker build -f "$PACK_IMAGE_DOCKERFILE" $noCacheFlag \
			 --build-arg BUILD_NUMBER="$BUILD_NUMBER" \
			 -t $ACR_PACK_IMAGE_REPO:latest \
			 .

if [ "$AGENT_BUILD" == "true" ]; then
	BUILD_SUFFIX="$BUILD_DEFINITIONNAME.$BUILD_NUMBER"

	docker tag "$ACR_PACK_IMAGE_REPO:latest" "$ACR_PACK_IMAGE_REPO:$BUILD_SUFFIX"
	echo "$ACR_PACK_IMAGE_REPO:$BUILD_SUFFIX" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
fi
