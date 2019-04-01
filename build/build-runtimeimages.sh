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
#   oryx/images/runtime/node/10.1.0/Dockerfile
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
    getTagName_result=${remainderPath//$slashChar/"-"}
    return 0
}

echo
echo "Generating Dockerfiles for Node runtime images..."
$REPO_DIR/images/runtime/node/generateDockerfiles.sh

echo
echo "Generating Dockerfiles for .NET Core runtime images..."
$REPO_DIR/images/runtime/dotnetcore/generateDockerfiles.sh

echo
echo "Generating Dockerfiles for Python runtime images..."
$REPO_DIR/images/runtime/python/generateDockerfiles.sh

echo
echo "Generating Dockerfiles for PHP runtime images..."
$REPO_DIR/images/runtime/php/generate-dockerfiles.sh

dockerFiles=$(find $RUNTIME_IMAGES_SRC_DIR -type f -name "Dockerfile")
if [ -z "$dockerFiles" ]
then
    echo "Couldn't find any Dockerfiles under '$RUNTIME_IMAGES_SRC_DIR' and its sub-directories."
    exit 1
fi

labels="--label com.microsoft.oryx.git-commit=$GIT_COMMIT --label com.microsoft.oryx.build-number=$BUILD_NUMBER"

# Avoid causing cache invalidation with the following check
if [ "$EMBED_BUILDCONTEXT_IN_IMAGES" == "true" ]
then
	args="--build-arg GIT_COMMIT=$GIT_COMMIT --build-arg BUILD_NUMBER=$BUILD_NUMBER"
fi

# Write the list of images that were built to artifacts folder
mkdir -p "$ARTIFACTS_DIR/images"

clearedOutput=false
for dockerFile in $dockerFiles; do
    dockerFileDir=$(dirname "${dockerFile}")
    getTagName $dockerFileDir
    localImageTagName="$LOCAL_RUNTIME_IMAGES_REPO_PREFIX/$getTagName_result:latest"
    
    echo
    echo "Building image '$localImageTagName' for docker file located at '$dockerFile'..."
    
    cd $REPO_DIR

    echo
    docker build -f $dockerFile -t $localImageTagName \
        --build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
        $args $labels .
    
    # Retag build image with DockerHub & ACR tags
    if [ -n "$BUILD_NUMBER" ]
    then
        uniqueTag="$BUILD_DEFINITIONNAME.$BUILD_NUMBER"

        dockerHubRuntimeImageTagNameRepo="$DOCKERHUB_RUNTIME_IMAGES_REPO_PREFIX/$getTagName_result"
        acrRuntimeImageTagNameRepo="$ACR_RUNTIME_IMAGES_REPO_PREFIX/$getTagName_result"

        docker tag "$localImageTagName" "$dockerHubRuntimeImageTagNameRepo:latest"
        docker tag "$localImageTagName" "$dockerHubRuntimeImageTagNameRepo:$uniqueTag"
        docker tag "$localImageTagName" "$acrRuntimeImageTagNameRepo:latest"
        docker tag "$localImageTagName" "$acrRuntimeImageTagNameRepo:$uniqueTag"

        if [ $clearedOutput == "false" ]
        then
            # clear existing contents of the file, if any
            > $DOCKERHUB_RUNTIME_IMAGES_ARTIFACTS_FILE
            > $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
            clearedOutput=true
        fi

        # add new content
        echo
        echo "Updating artifacts file with the built runtime image information..."
    	echo "$dockerHubRuntimeImageTagNameRepo:latest" >> $DOCKERHUB_RUNTIME_IMAGES_ARTIFACTS_FILE
    	echo "$dockerHubRuntimeImageTagNameRepo:$uniqueTag" >> $DOCKERHUB_RUNTIME_IMAGES_ARTIFACTS_FILE
        echo "$acrRuntimeImageTagNameRepo:latest" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
        echo "$acrRuntimeImageTagNameRepo:$uniqueTag" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
    fi

    cd $RUNTIME_IMAGES_SRC_DIR
done

if [ -n "$BUILD_NUMBER" ]
then
    echo
    echo "List of images built (from '$DOCKERHUB_RUNTIME_IMAGES_ARTIFACTS_FILE'):"
    cat $DOCKERHUB_RUNTIME_IMAGES_ARTIFACTS_FILE
    
    echo
    echo "List of images tagged (from '$ACR_RUNTIME_IMAGES_ARTIFACTS_FILE'):"
    cat $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
fi

echo
echo "Cleanup: Run 'docker system prune': $DOCKER_SYSTEM_PRUNE"
if [ "$DOCKER_SYSTEM_PRUNE" == "true" ]
then
	docker system prune -f
fi