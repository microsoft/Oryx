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
source $REPO_DIR/build/__pythonVersions.sh # For PYTHON_BASE_TAG
source $REPO_DIR/build/__phpVersions.sh    # For PHP_BUILD_BASE_TAG

cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"

declare BUILD_SIGNED=""

echo "SignType is: $SIGNTYPE"

# Check to see if the build is by scheduled ORYX-CI or other azure devops build
if [ "$SIGNTYPE" == "real" ] || [ "$SIGNTYPE" == "Real" ]
then
# "SignType" will be real only for builds by scheduled and/or manual builds  of ORYX-CI
	BUILD_SIGNED="true"
	ls -l $BUILD_IMAGES_BUILD_CONTEXT_DIR
else
	# locally we need to fake "binaries" directory to get a successful "copybuildscriptbinaries" build stage
    mkdir -p $BUILD_IMAGES_BUILD_CONTEXT_DIR/binaries
fi

# Avoid causing cache invalidation with the following check
if [ "$EMBED_BUILDCONTEXT_IN_IMAGES" == "true" ]
then
	ctxArgs="--build-arg GIT_COMMIT=$GIT_COMMIT --build-arg BUILD_NUMBER=$BUILD_NUMBER"
	echo "Build context args: $ctxArgs"
fi

function BuildAndTagStage()
{
	local dockerFile="$1"
	local stageName="$2"
	local stageTagName="$ACR_PUBLIC_PREFIX/$2"

	echo
	echo "Building stage '$stageName' with tag '$stageTagName'..."
	docker build --target $stageName -t $stageTagName $ctxArgs -f "$dockerFile" .
}

function buildDockerImage() {
	local dockerFileToBuild="$1"
	local dockerImageRepoName="$2"
	local dockerFileForTestsToBuild="$3"
	local dockerImageForTestsRepoName="$4"
	local dockerImageForDevelopmentRepoName="$5"

	# Tag stages to avoid creating dangling images.
	# NOTE:
	# These images are not written to artifacts file because they are not expected
	# to be pushed. This is just a workaround to prevent having dangling images so that
	# when a cleanup operation is being done on a build agent, a valuable dangling image
	# is not removed.
	BuildAndTagStage "$dockerFileToBuild" node-install
	BuildAndTagStage "$dockerFileToBuild" dotnet-install
	BuildAndTagStage "$dockerFileToBuild" python
	BuildAndTagStage "$dockerFileToBuild" buildscriptbuilder

	builtImageTag="$dockerImageRepoName:latest"
	docker build -t $builtImageTag \
		--build-arg AGENTBUILD=$BUILD_SIGNED \
		--build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
		$ctxArgs -f "$dockerFileToBuild" .

	echo
	echo Building a base image for tests...
	# Do not write this image tag to the artifacts file as we do not intend to push it
	docker build -t $dockerImageForTestsRepoName -f "$dockerFileForTestsToBuild" .
	
	echo "$dockerImageRepoName:latest" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE

	# Retag build image with build number tags
	if [ "$AGENT_BUILD" == "true" ]
	then
		uniqueTag="$BUILD_DEFINITIONNAME.$BUILD_NUMBER"

		echo
		echo "Retagging image '$builtImageTag' with ACR related tags..."
		docker tag "$builtImageTag" "$dockerImageRepoName:latest"
		docker tag "$builtImageTag" "$dockerImageRepoName:$uniqueTag"

		# Write the list of images that were built to artifacts folder
		echo
		echo "Writing the list of build images built to artifacts folder..."

		# Write image list to artifacts file
		echo "$dockerImageRepoName:$uniqueTag" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
	else
		docker tag "$builtImageTag" "$dockerImageForDevelopmentRepoName:latest"
	fi
}

docker pull buildpack-deps:stretch

# Create artifact dir & files
mkdir -p "$ARTIFACTS_DIR/images"

touch $ACR_BUILD_IMAGES_ARTIFACTS_FILE
> $ACR_BUILD_IMAGES_ARTIFACTS_FILE

echo
echo "-------------Creating slim build image-------------------"
buildDockerImage "$BUILD_IMAGES_SLIM_DOCKERFILE" \
				"$ACR_SLIM_BUILD_IMAGE_REPO" \
				"$ORYXTESTS_SLIM_BUILDIMAGE_DOCKERFILE" \
				"$ORYXTESTS_SLIM_BUILDIMAGE_REPO" \
				"$DEVBOX_SLIM_BUILD_IMAGE_REPO" 

echo
echo "-------------Creating full build image-------------------"
buildDockerImage "$BUILD_IMAGES_DOCKERFILE" \
				"$ACR_BUILD_IMAGES_REPO" \
				"$ORYXTESTS_BUILDIMAGE_DOCKERFILE" \
				"$ORYXTESTS_BUILDIMAGE_REPO" \
				"$DEVBOX_BUILD_IMAGES_REPO"

# Build buildpack images
# 'pack create-builder' is not supported on Windows
if [[ "$OSTYPE" == "linux-gnu" ]] || [[ "$OSTYPE" == "darwin"* ]]; then
	source $REPO_DIR/build/buildBuildpacksImages.sh
else
	echo
	echo "Skipping building Buildpacks images as platform '$OSTYPE' is not supported."
fi

echo
echo "List of images tagged (from '$ACR_BUILD_IMAGES_ARTIFACTS_FILE'):"
cat $ACR_BUILD_IMAGES_ARTIFACTS_FILE

echo
dockerCleanupIfRequested

if [ -z "$BUILD_SIGNED" ]
then
	rm -rf binaries
fi
