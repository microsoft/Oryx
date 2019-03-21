#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh

cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"

declare BUILDSCRIPT_SOURCE="buildscriptbuilder"
declare BUILD_SIGNED=""

# Check to see if the build is by scheduled ORYX-CI or other azure devops build
if [ "$SignType" == "real" ] || [ "$SignType" == "Real" ]
then
# "SignType" will be real only for builds by scheduled and/or manual builds  of ORYX-CI
    BUILDSCRIPT_SOURCE="copybuildscriptbinaries"
	BUILD_SIGNED="true"
else
# locally we need to fake "binaries" directory to get a successful "copybuildscriptbinaries" build stage
    mkdir -p $BUILD_IMAGES_BUILD_CONTEXT_DIR/binaries
fi

# Avoid causing cache invalidation with the following check
if [ "$EMBED_BUILDCONTEXT_IN_IMAGES" == "true" ]
then
	ctxArgs="--build-arg GIT_COMMIT=$GIT_COMMIT --build-arg BUILD_NUMBER=$BUILD_NUMBER"
	echo "build context: "$ctxArgs
fi

function BuildAndTagStage()
{
	local stageName="$1"
	local stageTagName="oryxdevms/$1"

	echo
	echo
	echo "Building stage '$stageName' with tag '$stageTagName'..."
	docker build --target $stageName -t $stageTagName $ctxArgs -f "$BUILD_IMAGES_DOCKERFILE" .
}

# Tag stages to avoid creating dangling images.
# NOTE:
# These images are not written to artifacts file because they are not expected
# to be pushed. This is just a workaround to prevent having dangling images so that
# when a cleanup operation is being done on a build agent, a valuable dangling image
# is not removed.
BuildAndTagStage node-install
BuildAndTagStage dotnet-install
BuildAndTagStage python-build-prereqs
BuildAndTagStage python2.7-build
BuildAndTagStage python3.5-build
BuildAndTagStage python3.6-build
BuildAndTagStage python3.7-build
BuildAndTagStage python
BuildAndTagStage buildscriptbuilder
BuildAndTagStage copybuildscriptbinaries
BuildAndTagStage buildscriptbinaries

tags="$DOCKER_BUILD_IMAGES_REPO:latest"

if [ -n "$BUILD_NUMBER" ]
then
    tags="$tags -t $DOCKER_BUILD_IMAGES_REPO:$BUILD_NUMBER"
fi

if [ -n "$BUILD_BUILDIMAGES_USING_NOCACHE" ]
then
	echo
	echo "Building build image(s) with NO cache..."
	noCache="--no-cache"
else
	echo
	echo "Building build image(s)..."
fi

echo "Application Insights instrumentation key: $APPLICATION_INSIGHTS_INSTRUMENTATION_KEY"
docker build $noCache -t $tags --build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY --build-arg AGENTBUILD=$BUILD_SIGNED --build-arg BUILDSCRIPT_SOURCE=$BUILDSCRIPT_SOURCE $ctxArgs -f "$BUILD_IMAGES_DOCKERFILE" .

echo
echo Building a base image for tests ...
# Do not write this image tag to the artifacts file as we do not intend to push it
docker build -t $ORYXTESTS_BUILDIMAGE_REPO -f "$ORYXTESTS_BUILDIMAGE_DOCKERFILE" .

# Retag build image with acr tags
docker tag "$DOCKER_BUILD_IMAGES_REPO:latest" "$ACR_BUILD_IMAGES_REPO:latest"

if [ -n "$BUILD_NUMBER" ]
then
    docker tag "$DOCKER_BUILD_IMAGES_REPO:latest" "$ACR_BUILD_IMAGES_REPO:$BUILD_NUMBER"
fi

# Write the list of images that were built to artifacts folder
echo
echo "Writing the list of build images built to artifacts folder..."
mkdir -p "$ARTIFACTS_DIR/images"

# Write image list to artifacts file
echo "$DOCKER_BUILD_IMAGES_REPO:latest" > $BUILD_IMAGES_ARTIFACTS_FILE
echo "$ACR_BUILD_IMAGES_REPO:latest" > $ACR_BUILD_IMAGES_ARTIFACTS_FILE

if [ -n "$BUILD_NUMBER" ]
then
	echo "$DOCKER_BUILD_IMAGES_REPO:$BUILD_NUMBER" >> $BUILD_IMAGES_ARTIFACTS_FILE
	echo "$ACR_BUILD_IMAGES_REPO:$BUILD_NUMBER" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
fi

echo
echo "List of images built (from '$BUILD_IMAGES_ARTIFACTS_FILE'):"
cat $BUILD_IMAGES_ARTIFACTS_FILE
echo "List of images tagged (from '$ACR_BUILD_IMAGES_ARTIFACTS_FILE'):"
cat $ACR_BUILD_IMAGES_ARTIFACTS_FILE

echo
echo "Cleanup: Run 'docker system prune': $DOCKER_SYSTEM_PRUNE"
if [ $DOCKER_SYSTEM_PRUNE = "true" ]
then
	docker system prune -f
fi

if [ -z "$BUILD_SIGNED" ]
then
	rm -rf binaries
fi
