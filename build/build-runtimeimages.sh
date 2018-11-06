#!/bin/bash
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
        echo "Expected to get a path to a directory containing a docker file, but didn``t get any."
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

labels="--label com.microsoft.oryx.git-commit=$GIT_COMMIT --label com.microsoft.oryx.build-number=$BUILD_NUMBER"

dockerFiles=$(find $RUNTIME_IMAGES_SRC_DIR -type f -name "Dockerfile")
if [ -z "$dockerFiles" ]
then
    echo "Couldn't find any docker files under '$RUNTIME_IMAGES_SRC_DIR' and it's sub-directories."
    exit 1
fi

# Write the list of images that were built to artifacts folder
mkdir -p "$ARTIFACTS_DIR/images"

clearedOutput=false
for dockerFile in $dockerFiles; do
    dockerFileDir=$(dirname "${dockerFile}")
    getTagName $dockerFileDir
    echo $getTagName
    echo $getTagName_result
    runtimeImageTagName="$DOCKER_RUNTIME_IMAGES_REPO/$getTagName_result"
    runtimeImageACRTagName="$ACR_RUNTIME_IMAGES_REPO/$getTagName_result"

    tags=$runtimeImageTagName:latest
    acrTag=$runtimeImageACRTagName:latest

    if [ -n "$BUILD_NUMBER" ]
    then
        tags="$tags -t $runtimeImageTagName:$BUILD_NUMBER"
    fi
    
    echo
    echo "Building image '$runtimeImageTagName' for docker file located at '$dockerFile'..."
    echo
    
    cd $dockerFileDir

    if [ -n "$BUILD_RUNTIMEIMAGES_USING_NOCACHE" ]
    then
        echo "Building image '$runtimeImageTagName' with NO cache..."
        noCache="--no-cache"
    else
        echo "Building image '$runtimeImageTagName'..."
    fi
    echo
    docker build $noCache -t $tags $labels .
    
    # Retag build image with acr tags
    docker tag "$runtimeImageTagName:latest" "$runtimeImageACRTagName:latest"

    if [ -n "$BUILD_NUMBER" ]
    then
        docker tag "$runtimeImageTagName:latest" "$runtimeImageACRTagName:$BUILD_NUMBER"
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
    	echo "$runtimeImageTagName:$BUILD_NUMBER" >> $RUNTIME_IMAGES_ARTIFACTS_FILE
        echo "$runtimeImageACRTagName:$BUILD_NUMBER" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE
    fi

    cd $RUNTIME_IMAGES_SRC_DIR
done

echo
echo "List of images built (from '$RUNTIME_IMAGES_ARTIFACTS_FILE'):"
cat $RUNTIME_IMAGES_ARTIFACTS_FILE
echo "List of images tagged (from '$ACR_RUNTIME_IMAGES_ARTIFACTS_FILE'):"
cat $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE

echo
echo "Cleanup: Run 'docker system prune': $DOCKER_SYSTEM_PRUNE"
if [ $DOCKER_SYSTEM_PRUNE = "true" ]
then
	docker system prune -f
fi