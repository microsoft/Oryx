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

labels="--label com.microsoft.oryx.git-commit=$GIT_COMMIT --label com.microsoft.oryx.build-number=$BUILD_NUMBER"

# Avoid causing cache invalidation with the following check
if [ "$EMBED_BUILDCONTEXT_IN_IMAGES" == "true" ]
then
	args="--build-arg GIT_COMMIT=$GIT_COMMIT --build-arg BUILD_NUMBER=$BUILD_NUMBER"
fi

dockerFiles=$(find $RUNTIME_IMAGES_SRC_DIR -type f -name "Dockerfile")
if [ -z "$dockerFiles" ]
then
    echo "Couldn't find any Dockerfiles under '$RUNTIME_IMAGES_SRC_DIR' and its sub-directories."
    exit 1
fi

# Write the list of images that were built to artifacts folder
mkdir -p "$ARTIFACTS_DIR/images"

clearedOutput=false
for dockerFile in $dockerFiles; do
    dockerFileDir=$(dirname "${dockerFile}")
    getTagName $dockerFileDir
    runtimeImageTagName="$DOCKER_RUNTIME_IMAGES_REPO/$getTagName_result"
    runtimeImageACRTagName="$ACR_RUNTIME_IMAGES_REPO/$getTagName_result"

    tags=$runtimeImageTagName:latest
    acrTag=$runtimeImageACRTagName:latest

    if [ -n "$BUILD_NUMBER" ]
    then
        tags="$tags -t $runtimeImageTagName:$BUILD_DEFINITIONNAME.$BUILD_NUMBER"
    fi
    
    echo
    echo "Building image '$runtimeImageTagName' for docker file located at '$dockerFile'..."
    
    cd $REPO_DIR

    if [ -n "$BUILD_RUNTIMEIMAGES_USING_NOCACHE" ]
    then
        echo "Building image '$runtimeImageTagName' with NO cache..."
        noCache="--no-cache"
    fi

    echo
    docker build $noCache -f $dockerFile -t $tags --build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY $args $labels .
    
    # Retag build image with acr tags
    docker tag "$runtimeImageTagName:latest" "$runtimeImageACRTagName:latest"

    if [ -n "$BUILD_NUMBER" ]
    then
        docker tag "$runtimeImageTagName:latest" "$runtimeImageACRTagName:$BUILD_DEFINITIONNAME.$BUILD_NUMBER"
    fi

    if [ $clearedOutput = "false" ]
    then
        # clear existing contents of the file, if any
        > $RUNTIME_IMAGES_ARTIFACTS_FILE
        > $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
        clearedOutput=true
    fi

    # add new content
    echo
    echo "Updating artifacts file with the built runtime image information..."
    echo "$runtimeImageTagName:latest" >> $RUNTIME_IMAGES_ARTIFACTS_FILE
    echo "$runtimeImageACRTagName:latest" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE

    if [ -n "$BUILD_NUMBER" ]
    then
    	echo "$runtimeImageTagName:$BUILD_DEFINITIONNAME.$BUILD_NUMBER" >> $RUNTIME_IMAGES_ARTIFACTS_FILE
        echo "$runtimeImageACRTagName:$BUILD_DEFINITIONNAME.$BUILD_NUMBER" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
    fi

    cd $RUNTIME_IMAGES_SRC_DIR
done

echo
echo "List of images built (from '$RUNTIME_IMAGES_ARTIFACTS_FILE'):"
cat $RUNTIME_IMAGES_ARTIFACTS_FILE
echo
echo "List of images tagged (from '$ACR_RUNTIME_IMAGES_ARTIFACTS_FILE'):"
cat $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE

echo
echo "Cleanup: Run 'docker system prune': $DOCKER_SYSTEM_PRUNE"
if [ "$DOCKER_SYSTEM_PRUNE" == "true" ]
then
	docker system prune -f
fi