#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__functions.sh
source $REPO_DIR/build/__sdkStorageConstants.sh

runtimeImagesSourceDir="$RUNTIME_IMAGES_SRC_DIR"
runtimeSubDir=""
runtimeImageDebianFlavor=""

if [ $# -eq 2 ] 
then
    echo "Locally building runtime '$runtimeSubDir'"
    runtimeSubDir="$1"
    runtimeImageDebianFlavor="$2"
elif [ $# -eq 1 ] 
    echo "CI Agent building runtime '$runtimeSubDir'"
    runtimeImageDebianFlavor="$1"
fi

if [ ! -z "$runtimeSubDir" ]
then
    runtimeImagesSourceDir="$runtimeImagesSourceDir/$runtimeSubDir"
    if [ ! -d "$runtimeImagesSourceDir" ]; then
        (>&2 echo "Unknown runtime '$runtimeSubDir'")
        exit 1
    fi
fi

labels="--label com.microsoft.oryx.git-commit=$GIT_COMMIT"
labels="$labels --label com.microsoft.oryx.build-number=$BUILD_NUMBER"
labels="$labels --label com.microsoft.oryx.release-tag-name=$RELEASE_TAG_NAME"

# Avoid causing cache invalidation with the following check
if [ "$EMBED_BUILDCONTEXT_IN_IMAGES" == "true" ]
then
    args="--build-arg GIT_COMMIT=$GIT_COMMIT"
    args="$args --build-arg BUILD_NUMBER=$BUILD_NUMBER"
    args="$args --build-arg RELEASE_TAG_NAME=$RELEASE_TAG_NAME"
fi

# Build the common base image first, so other images that depend on it get the latest version.
# We don't retrieve this image from a repository but rather build locally to make sure we get
# the latest version of its own base image.

docker build \
    --pull \
    -f "$RUNTIME_BASE_IMAGE_DOCKERFILE_PATH" \
    -t "$RUNTIME_BASE_IMAGE_NAME-stretch" \
    --build-arg DEBIAN_FLAVOR=stretch \
    $REPO_DIR

docker build \
    --pull \
    -f "$RUNTIME_BASE_IMAGE_DOCKERFILE_PATH" \
    -t "$RUNTIME_BASE_IMAGE_NAME-buster" \
    --build-arg DEBIAN_FLAVOR=buster \
    $REPO_DIR

execAllGenerateDockerfiles "$runtimeImagesSourceDir" "generateDockerfiles.sh" "$runtimeImageDebianFlavor"

# The common base image is built separately, so we ignore it
dockerFiles=$(find $runtimeImagesSourceDir -type f \( -name "$runtimeImageDebianFlavor.Dockerfile" ! -path "$RUNTIME_IMAGES_SRC_DIR/commonbase/*" \) )
if [ -z "$dockerFiles" ]
then
    echo "Couldn't find any Dockerfiles under '$runtimeImagesSourceDir' and its sub-directories."
    exit 1
fi

# Write the list of images that were built to artifacts folder
mkdir -p "$ARTIFACTS_DIR/images"

if [ "$AGENT_BUILD" == "true" ]
then
    # clear existing contents of the file, if any
    > $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
fi

for dockerFile in $dockerFiles; do
    dockerFileDir=$(dirname "${dockerFile}")

    # Set $getTagName_result to the following format: {platformName}:{platformVersion}
    getTagName $dockerFileDir

    # Set $localImageTagName to the following format: oryxdevmcr.azurecr.io/public/oryx/{platformName}:{platformVersion}
    localImageTagName="$ACR_PUBLIC_PREFIX/$getTagName_result-$runtimeImageDebianFlavor"

    echo
    echo "Building image '$localImageTagName' for Dockerfile located at '$dockerFile'..."

    cd $REPO_DIR

    echo
    docker build \
        -f $dockerFile \
        -t $localImageTagName \
        --build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
        --build-arg SDK_STORAGE_ENV_NAME=$SDK_STORAGE_BASE_URL_KEY_NAME \
        --build-arg SDK_STORAGE_BASE_URL_VALUE=$PROD_SDK_CDN_STORAGE_BASE_URL \
        --build-arg DEBIAN_FLAVOR=$runtimeImageDebianFlavor \
        $args \
        $labels \
        .

    echo "$localImageTagName" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE

    # Retag image with build number (for images built in oryxlinux buildAgent)
    if [ "$AGENT_BUILD" == "true" ]
    then
        # $uniqueTag will follow a similar format to Oryx-CI.20191028.1
        # $BUILD_DEFINITIONNAME is the name of the build (e.g., Oryx-CI)
        # $RELEASE_TAG_NAME is either the date of the build if the branch is master, or
        # the name of the branch the build is against
        uniqueTag="$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"

        # Set $acrRuntimeImageTagNameRepo to the following format: oryxdevmcr.azurecr.io/public/oryx/{platformName}:{platformVersion}
        acrRuntimeImageTagNameRepo="$ACR_PUBLIC_PREFIX/$getTagName_result"

        # Tag the image to follow a similar format to .../python:3.7-Oryx-CI.20191028.1
        docker tag "$localImageTagName" "$acrRuntimeImageTagNameRepo-$uniqueTag-$runtimeImageDebianFlavor"

        # add new content
        echo
        echo "Updating runtime image artifacts file with build number..."
        echo "$acrRuntimeImageTagNameRepo-$uniqueTag" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
    else
        devBoxRuntimeImageTagNameRepo="$DEVBOX_RUNTIME_IMAGES_REPO_PREFIX/$getTagName_result"
        docker tag "$localImageTagName" "$devBoxRuntimeImageTagNameRepo"
    fi

    cd $RUNTIME_IMAGES_SRC_DIR
done

if [ "$AGENT_BUILD" == "true" ]
then
    echo
    echo "List of images tagged (from '$ACR_RUNTIME_IMAGES_ARTIFACTS_FILE'):"
    cat $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
fi

echo
showDockerImageSizes

echo
dockerCleanupIfRequested
