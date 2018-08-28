#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh

tags="-t $DOCKER_BUILD_IMAGES_REPO:latest"

if [ -n "$BUILD_NUMBER" ]
then
    tags="$tags -t $DOCKER_BUILD_IMAGES_REPO:$BUILD_NUMBER"
fi

echo
echo "Building build image(s)..."
echo

cd $BUILD_IMAGES_SRC_DIR
docker build $tags .

# Write the list of images that were built to artifacts folder
echo
echo "Writing the list of build images built to artifacts folder..."
mkdir -p "$ARTIFACTS_DIR"

# Write image list to artifacts file
echo "$DOCKER_BUILD_IMAGES_REPO:latest" > $BUILD_IMAGES_ARTIFACTS_FILE
echo "$DOCKER_BUILD_IMAGES_REPO:$BUILD_NUMBER" >> $BUILD_IMAGES_ARTIFACTS_FILE

echo
echo "List of images built (from '$BUILD_IMAGES_ARTIFACTS_FILE'):"
cat $BUILD_IMAGES_ARTIFACTS_FILE