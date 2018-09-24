#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r GIT_COMMIT=$(git rev-parse HEAD)

# Load all variables
source $REPO_DIR/build/__variables.sh

tags="-t $DOCKER_BUILD_IMAGES_REPO:latest"
labels="--label com.microsoft.oryx.git-commit=$GIT_COMMIT --label com.microsoft.oryx.build-number=$BUILD_NUMBER"

if [ -n "$BUILD_NUMBER" ]
then
    tags="$tags -t $DOCKER_BUILD_IMAGES_REPO:$BUILD_NUMBER"
fi

cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"

if [ -n "$BUILD_BUILDIMAGES_USING_NOCACHE" ]
then
	echo
	echo "Building build image(s) with NO cache..."
	docker build --no-cache $tags $labels . -f "$BUILD_IMAGES_DOCKERFILE"
else
	echo
	echo "Building build image(s)..."
	docker build $tags $labels . -f "$BUILD_IMAGES_DOCKERFILE"
fi

# Write the list of images that were built to artifacts folder
echo
echo "Writing the list of build images built to artifacts folder..."
mkdir -p "$ARTIFACTS_DIR/images"

# Write image list to artifacts file
echo "$DOCKER_BUILD_IMAGES_REPO:latest" > $BUILD_IMAGES_ARTIFACTS_FILE

if [ -n "$BUILD_NUMBER" ]
then
	echo "$DOCKER_BUILD_IMAGES_REPO:$BUILD_NUMBER" >> $BUILD_IMAGES_ARTIFACTS_FILE
fi

echo
echo "List of images built (from '$BUILD_IMAGES_ARTIFACTS_FILE'):"
cat $BUILD_IMAGES_ARTIFACTS_FILE