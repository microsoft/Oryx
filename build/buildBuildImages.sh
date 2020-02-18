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
source $REPO_DIR/build/__nodeVersions.sh   # For YARN_CACHE_BASE_TAG
source $REPO_DIR/build/__nodeVersions.sh   # For YARN_CACHE_BASE_TAG
source $REPO_DIR/build/__sdkStorageConstants.sh

declare -r BASE_TAG_BUILD_ARGS="--build-arg PYTHON_BASE_TAG=$PYTHON_BASE_TAG \
                                --build-arg PHP_BUILD_BASE_TAG=$PHP_BUILD_BASE_TAG \
                                --build-arg YARN_CACHE_BASE_TAG=$YARN_CACHE_BASE_TAG" \

echo
echo Base tag args used:
echo $BASE_TAG_BUILD_ARGS
echo

cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"

declare BUILD_SIGNED=""

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
	ctxArgs="--build-arg GIT_COMMIT=$GIT_COMMIT"
	ctxArgs="$ctxArgs --build-arg BUILD_NUMBER=$BUILD_NUMBER"
	ctxArgs="$ctxArgs --build-arg RELEASE_TAG_NAME=$RELEASE_TAG_NAME"
	echo "Build context args: $ctxArgs"
fi

function BuildAndTagStage()
{
	local dockerFile="$1"
	local stageName="$2"
	local stageTagName="$ACR_PUBLIC_PREFIX/$2"

	echo
	echo "Building stage '$stageName' with tag '$stageTagName'..."
	docker build --target $stageName -t $stageTagName $ctxArgs $BASE_TAG_BUILD_ARGS -f "$dockerFile" .
}

function buildDockerImage() {
	local dockerFileToBuild="$1"
	local dockerImageRepoName="$2"
	local dockerFileForTestsToBuild="$3"
	local dockerImageForTestsRepoName="$4"
	local dockerImageForDevelopmentRepoName="$5"
	local dockerImageBaseTag="$6"

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

	# If no tag was provided, use a default tag of "latest"
	if [ -z "$dockerImageBaseTag" ]
	then
		dockerImageBaseTag="latest"
	fi

	builtImageTag="$dockerImageRepoName:$dockerImageBaseTag"
	docker build -t $builtImageTag \
		--build-arg AGENTBUILD=$BUILD_SIGNED \
		$BASE_TAG_BUILD_ARGS \
		--build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
		--build-arg SDK_STORAGE_ENV_NAME=$SDK_STORAGE_BASE_URL_KEY_NAME \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$PROD_SDK_STORAGE_BASE_URL \
		$ctxArgs \
		-f "$dockerFileToBuild" \
		.

	echo
	echo Building a base image for tests...
	# Do not write this image tag to the artifacts file as we do not intend to push it
	testImageTag="$dockerImageForTestsRepoName:$dockerImageBaseTag"
	docker build -t $testImageTag -f "$dockerFileForTestsToBuild" .

	echo "$dockerImageRepoName:$dockerImageBaseTag" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE

	# Retag build image with build number tags
	if [ "$AGENT_BUILD" == "true" ]
	then
		uniqueTag="$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"
		if [ "$dockerImageBaseTag" != "latest" ]
		then
			uniqueTag="$dockerImageBaseTag-$uniqueTag"
		fi

		echo
		echo "Retagging image '$builtImageTag' with ACR related tags..."
		docker tag "$builtImageTag" "$dockerImageRepoName:$dockerImageBaseTag"
		docker tag "$builtImageTag" "$dockerImageRepoName:$uniqueTag"

		# Write the list of images that were built to artifacts folder
		echo
		echo "Writing the list of build images built to artifacts folder..."

		# Write image list to artifacts file
		echo "$dockerImageRepoName:$uniqueTag" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
	else
		docker tag "$builtImageTag" "$dockerImageForDevelopmentRepoName:$dockerImageBaseTag"
	fi
}

docker pull buildpack-deps:stretch

# Create artifact dir & files
mkdir -p "$ARTIFACTS_DIR/images"

touch $ACR_BUILD_IMAGES_ARTIFACTS_FILE
> $ACR_BUILD_IMAGES_ARTIFACTS_FILE

echo
echo "-------------Creating AzureFunctions JamStack image-------------------"
builtImageName="$ACR_AZURE_FUNCTIONS_JAMSTACK_IMAGE_NAME"
docker build -t $builtImageName \
	--build-arg AGENTBUILD=$BUILD_SIGNED \
	$BASE_TAG_BUILD_ARGS \
	--build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
	$ctxArgs \
	-f "$BUILD_IMAGES_AZ_FUNCS_JAMSTACK_DOCKERFILE" \
	.
echo
echo "$builtImageName" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE

# Retag build image with build number tags
if [ "$AGENT_BUILD" == "true" ]
then
	uniqueImageName="$builtImageName-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"

	echo
	echo "Retagging image '$builtImageName' with ACR related tags..."
	docker tag "$builtImageName" "$uniqueImageName"

	# Write image list to artifacts file
	echo "$uniqueImageName" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
fi

echo
echo "-------------Creating slim build image-------------------"
buildDockerImage "$BUILD_IMAGES_SLIM_DOCKERFILE" \
				"$ACR_BUILD_IMAGES_REPO" \
				"$ORYXTESTS_SLIM_BUILDIMAGE_DOCKERFILE" \
				"$ORYXTESTS_BUILDIMAGE_REPO" \
				"$DEVBOX_BUILD_IMAGES_REPO" \
				"slim"

echo
echo "-------------Creating full build image-------------------"
buildDockerImage "$BUILD_IMAGES_DOCKERFILE" \
				"$ACR_BUILD_IMAGES_REPO" \
				"$ORYXTESTS_BUILDIMAGE_DOCKERFILE" \
				"$ORYXTESTS_BUILDIMAGE_REPO" \
				"$DEVBOX_BUILD_IMAGES_REPO"

echo
echo "-------------Creating CLI image-------------------"
builtImageTag="$ACR_CLI_BUILD_IMAGE_REPO:latest"
docker build -t $builtImageTag \
	--build-arg AGENTBUILD=$BUILD_SIGNED \
	$BASE_TAG_BUILD_ARGS \
	--build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
	$ctxArgs \
	-f "$BUILD_IMAGES_CLI_DOCKERFILE" \
	.
echo
echo "$builtImageTag" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE

# Retag build image with build number tags
if [ "$AGENT_BUILD" == "true" ]
then
	uniqueTag="$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"

	echo
	echo "Retagging image '$builtImageTag' with ACR related tags..."
	docker tag "$builtImageTag" "$ACR_CLI_BUILD_IMAGE_REPO:latest"
	docker tag "$builtImageTag" "$ACR_CLI_BUILD_IMAGE_REPO:$uniqueTag"

	# Write the list of images that were built to artifacts folder
	echo
	echo "Writing the list of build images built to artifacts folder..."

	# Write image list to artifacts file
	echo "$ACR_CLI_BUILD_IMAGE_REPO:$uniqueTag" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
else
	docker tag "$builtImageTag" "$DEVBOX_CLI_BUILD_IMAGE_REPO:latest"
fi

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
showDockerImageSizes

echo
dockerCleanupIfRequested

if [ -z "$BUILD_SIGNED" ]
then
	rm -rf binaries
fi
