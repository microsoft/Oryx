#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

if [ -z $REPO_DIR ]; then
	declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
fi

if [ -z $_LOADED_COMMON_VARIABLES ]; then
	source $REPO_DIR/build/__variables.sh
fi

if [ -n "$BUILD_BUILDIMAGES_USING_NOCACHE" ]; then
	echo
	echo "Building buildpack runner image with NO cache..."
	noCacheFlag='--no-cache'
else
	echo
	echo "Building buildpack runner image..."
fi

labels="--label com.microsoft.oryx.git-commit=$GIT_COMMIT"
labels="$labels --label com.microsoft.oryx.build-number=$BUILD_NUMBER"
labels="$labels --label com.microsoft.oryx.release-tag-name=$RELEASE_TAG_NAME"
labels="$labels --label pack-tool-version=$PACK_TOOL_VERSION"

# Build an image that runs `pack`
echo "-> Building pack runner image: $ACR_PACK_IMAGE_REPO"
echo
cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"
docker build -f "$PACK_IMAGE_DOCKERFILE" $noCacheFlag \
			 $labels \
			 -t $ACR_PACK_IMAGE_REPO:latest \
			 .

if [ "$AGENT_BUILD" == "true" ]; then
	BUILD_SUFFIX="$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"

	docker tag "$ACR_PACK_IMAGE_REPO:latest" "$ACR_PACK_IMAGE_REPO:$BUILD_SUFFIX"
	echo "$ACR_PACK_IMAGE_REPO:$BUILD_SUFFIX" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
fi
