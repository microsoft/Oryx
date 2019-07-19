#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__nodeVersions.sh
source $REPO_DIR/build/__functions.sh


runtimeImagesSourceDir="$RUNTIME_IMAGES_SRC_DIR"
runtimeSubDir="$1"
if [ ! -z "$runtimeSubDir" ]
then
    runtimeImagesSourceDir="$runtimeImagesSourceDir/$runtimeSubDir"
    if [ ! -d "$runtimeImagesSourceDir" ]; then
        (>&2 echo "Unknown runtime '$runtimeSubDir'")
        exit 1
    fi
fi

labels="--label com.microsoft.oryx.git-commit=$GIT_COMMIT --label com.microsoft.oryx.build-number=$BUILD_NUMBER"

generateDockerFiles=$(find $runtimeImagesSourceDir -type f -name "generateDockerfiles.sh")
if [ -z "$generateDockerFiles" ]
then
    echo "Couldn't find any 'generateDockerfiles.sh' under '$runtimeImagesSourceDir' and its sub-directories."
fi

for generateDockerFile in $generateDockerFiles; do
    echo
    echo "Executing '$generateDockerFile'..."
    "$generateDockerFile"
done

dockerFileName="Dockerfile.base"
dockerFiles=$(find $runtimeImagesSourceDir -type f -name $dockerFileName)
if [ -z "$dockerFiles" ]
then
    echo "Couldn't find any Dockerfiles with name '$dockerFileName' under '$runtimeImagesSourceDir' and its sub-directories."
    exit 1
fi

# Build the common base image first, so other images that depend on it get the latest version.
# We don't retrieve this image from a repository but rather build locally to make sure we get
# the latest version of its own base image.
docker build --pull -f "$RUNTIME_BASE_IMAGE_DOCKERFILE_PATH" -t "$RUNTIME_BASE_IMAGE_NAME" $REPO_DIR

# Write the list of images that were built to artifacts folder
mkdir -p "$BASE_IMAGES_ARTIFACTS_FILE_PREFIX"
ARTIFACTS_FILE="$BASE_IMAGES_ARTIFACTS_FILE_PREFIX/$runtimeSubDir-runtimeimage-bases.txt"

clearedOutput=false
for dockerFile in $dockerFiles; do
    dockerFileDir=$(dirname "${dockerFile}")
    getTagName $dockerFileDir
    localImageTagName="$ACR_PUBLIC_PREFIX/$getTagName_result:latest"
    
    echo
    echo "Building image '$localImageTagName' for Dockerfile located at '$dockerFile'..."
    
    cd $REPO_DIR

    echo
    docker build -f $dockerFile \
        -t $localImageTagName \
        --build-arg CACHEBUST=$(date +%s) \
        --build-arg NODE6_VERSION=$NODE6_VERSION \
        --build-arg NODE8_VERSION=$NODE8_VERSION \
        --build-arg NODE10_VERSION=$NODE10_VERSION \
        --build-arg NODE12_VERSION=$NODE12_VERSION \
        $labels . 

    # Retag build image with build numbers as ACR tags
    if [ "$AGENT_BUILD" == "true" ]
    then
        tag="$BUILD_NUMBER"

        acrRuntimeImageTagNameRepo="$ACR_PUBLIC_PREFIX/$getTagName_result"

        docker tag "$localImageTagName" "$acrRuntimeImageTagNameRepo:$tag"

        if [ $clearedOutput == "false" ]
        then
            # clear existing contents of the file, if any
            > $ARTIFACTS_FILE
            clearedOutput=true
        fi

        # add new content
        echo
        echo "Updating artifacts file with the built runtime image information..."
        echo "$acrRuntimeImageTagNameRepo:$tag" >> $ARTIFACTS_FILE
    fi

    cd $RUNTIME_IMAGES_SRC_DIR
done

if [ "$AGENT_BUILD" == "true" ]
then
    echo
    echo "List of images tagged (from '$ARTIFACTS_FILE'):"
    cat $ARTIFACTS_FILE
fi

echo
dockerCleanupIfRequested
