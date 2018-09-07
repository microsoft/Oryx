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
    runtimeImageTagName="$DOCKER_RUNTIME_IMAGES_REPO/$getTagName_result"

    tags=$runtimeImageTagName

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
        echo
        docker build --no-cache -t $tags .
    else
        echo "Building image '$runtimeImageTagName'..."
        echo
        docker build -t $tags .
    fi

    if [ $clearedOutput = "false" ]
    then
        # clear existing contents of the file, if any
        > $RUNTIME_IMAGES_ARTIFACTS_FILE
        clearedOutput=true
    fi

    # add new content
    echo
    echo "Updating artifacts file with the built runtime image information..."
    echo "$runtimeImageTagName:latest" >> $RUNTIME_IMAGES_ARTIFACTS_FILE
    if [ -n "$BUILD_NUMBER" ]
    then
    	echo "$runtimeImageTagName:$BUILD_NUMBER" >> $RUNTIME_IMAGES_ARTIFACTS_FILE
    fi

    cd $RUNTIME_IMAGES_SRC_DIR
done

echo
echo "List of images built (from '$RUNTIME_IMAGES_ARTIFACTS_FILE'):"
cat $RUNTIME_IMAGES_ARTIFACTS_FILE