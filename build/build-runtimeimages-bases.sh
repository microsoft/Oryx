#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh

# Folder structure is used to come up with the tag name
# For example, if a docker file was located at
#   oryx/images/runtime/node/10.1.0/Dockerfile.base
# Then the tag name would be 'node-10.1.0'(i.e the path between 'runtime' and 'Dockerfile' segments)
function getTagName()
{
    if [ ! $# -eq 1 ]
    then
        echo "Expected to get a path to a directory containing a docker file, but did not get any."
        return 1
    fi

    if [ ! -d $1 ]
    then
        echo "Directory '$1' does not exist."
        return 1
    fi

    local replacedPath="$RUNTIME_IMAGES_SRC_DIR/"
    local remainderPath="${1//$replacedPath/}"
    local slashChar="/"
    getTagName_result="${remainderPath//$slashChar/"-"}-base"
    return 0
}

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

# Write the list of images that were built to artifacts folder
mkdir -p "$BASE_IMAGES_ARTIFACTS_FILE_PREFIX"
ARTIFACTS_FILE="$BASE_IMAGES_ARTIFACTS_FILE_PREFIX/$runtimeSubDir-runtimeimage-bases.txt"

clearedOutput=false
for dockerFile in $dockerFiles; do
    dockerFileDir=$(dirname "${dockerFile}")
    getTagName $dockerFileDir
    localImageTagName="$DOCKER_RUNTIME_IMAGES_REPO/$getTagName_result:latest"
    
    echo
    echo "Building image '$localImageTagName' for docker file located at '$dockerFile'..."
    
    cd $REPO_DIR

    echo
    docker build -f $dockerFile -t $localImageTagName $labels .

    # Retag build image with DockerHub & ACR tags
    if [ -n "$BUILD_NUMBER" ]
    then
        tag="$BUILD_NUMBER"

        acrRuntimeImageTagNameRepo="$ACR_RUNTIME_IMAGES_REPO/$getTagName_result"

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

if [ -n "$BUILD_NUMBER" ]
then
    echo
    echo "List of images tagged (from '$ARTIFACTS_FILE'):"
    cat $ARTIFACTS_FILE
fi

echo
echo "Cleanup: Run 'docker system prune': $DOCKER_SYSTEM_PRUNE"
if [ "$DOCKER_SYSTEM_PRUNE" == "true" ]
then
	docker system prune -f
fi