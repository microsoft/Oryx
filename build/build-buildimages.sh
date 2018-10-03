#!/bin/bash
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )
declare -r GIT_COMMIT=$(git rev-parse HEAD)

# Load all variables
source $REPO_DIR/build/__variables.sh

cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"

function BuildAndTagStage(){
	local stageName="$1"
	local stageTagName="oryxdevms/$1"

	echo
	echo
	echo "Building stage '$stageName' with tag '$stageTagName' ..."
	docker build --target $stageName -t $stageTagName -f "$BUILD_IMAGES_DOCKERFILE" .
}

# Tag stages to avoid creating dangling images.
# NOTE:
# These images are not written to artifacts file because they are not expected
# to be pushed. This is just a workaround to prevent having dangling images so that
# when a cleanup operation is being done on a build agent, a valuable dangling image
# is not removed.
BuildAndTagStage python-build-prereqs
BuildAndTagStage python2.7.15-build
BuildAndTagStage python3.5.6-build
BuildAndTagStage python3.6.6-build
BuildAndTagStage openssl1.1.1-build
BuildAndTagStage python3.7.0-build
BuildAndTagStage buildscriptbuilder

tags="-t $DOCKER_BUILD_IMAGES_REPO:latest"
labels="--label com.microsoft.oryx.git-commit=$GIT_COMMIT --label com.microsoft.oryx.build-number=$BUILD_NUMBER"

if [ -n "$BUILD_NUMBER" ]
then
    tags="$tags -t $DOCKER_BUILD_IMAGES_REPO:$BUILD_NUMBER"
fi

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

echo
echo "Cleanup: Run 'docker system prune': $DOCKER_SYSTEM_PRUNE"
if [ $DOCKER_SYSTEM_PRUNE = "true" ]
then
	docker system prune -f
fi
