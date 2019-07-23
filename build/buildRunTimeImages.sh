#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh
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

# Avoid causing cache invalidation with the following check
if [ "$EMBED_BUILDCONTEXT_IN_IMAGES" == "true" ]
then
	args="--build-arg GIT_COMMIT=$GIT_COMMIT --build-arg BUILD_NUMBER=$BUILD_NUMBER"
fi

execAllGenerateDockerfiles "$runtimeImagesSourceDir"

# The common base image is built separately, so we ignore it
dockerFiles=$(find $runtimeImagesSourceDir -type f \( -name "Dockerfile" ! -path "$RUNTIME_IMAGES_SRC_DIR/commonbase/*" \) )
if [ -z "$dockerFiles" ]
then
    echo "Couldn't find any Dockerfiles under '$runtimeImagesSourceDir' and its sub-directories."
    exit 1
fi

# Build the common base image first, so other images that depend on it get the latest version. 
# We don't retrieve this image from a repository but rather build locally to make sure we get 
# the latest version of its own base image. 

docker build --pull -f "$RUNTIME_BASE_IMAGE_DOCKERFILE_PATH" -t "$RUNTIME_BASE_IMAGE_NAME" $REPO_DIR

# Write the list of images that were built to artifacts folder
mkdir -p "$ARTIFACTS_DIR/images"

clearedOutput=false
for dockerFile in $dockerFiles; do
    dockerFileDir=$(dirname "${dockerFile}")
    getTagName $dockerFileDir
    localImageTagName="$ACR_PUBLIC_PREFIX/$getTagName_result:latest"
    
    echo
    echo "Building image '$localImageTagName' for Dockerfile located at '$dockerFile'..."
    
    cd $REPO_DIR

    echo
    docker build -f $dockerFile -t $localImageTagName \
        --build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
        $args $labels .

    echo "$localImageTagName" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE

    # Retag image with build number (for images built in oryxlinux buildAgent)
    if [ "$AGENT_BUILD" == "true" ]
    then
        uniqueTag="$BUILD_DEFINITIONNAME.$BUILD_NUMBER"
        acrRuntimeImageTagNameRepo="$ACR_PUBLIC_PREFIX/$getTagName_result"

        docker tag "$localImageTagName" "$acrRuntimeImageTagNameRepo:$uniqueTag"

        if [ $clearedOutput == "false" ]
        then
            # clear existing contents of the file, if any
            > $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
            clearedOutput=true
        fi

        # add new content
        echo
        echo "Updating runtime image artifacts file with build number..."
        echo "$acrRuntimeImageTagNameRepo:$uniqueTag" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
    else
        devBoxRuntimeImageTagNameRepo="$DEVBOX_RUNTIME_IMAGES_REPO_PREFIX/$getTagName_result"
        docker tag "$localImageTagName" "$devBoxRuntimeImageTagNameRepo:latest"
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
dockerCleanupIfRequested
