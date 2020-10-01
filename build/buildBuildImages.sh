#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__functions.sh
source $REPO_DIR/build/__sdkStorageConstants.sh
source $REPO_DIR/build/__nodeVersions.sh # for YARN_CACHE_BASE_TAG

cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"

declare BUILD_SIGNED=""

# Check to see if the build is by scheduled ORYX-CI or other azure devops build
# SIGNTYPE is set to 'real' on the Oryx-CI build definition itself (not in yaml file)
if [ "$SIGNTYPE" == "real" ] || [ "$SIGNTYPE" == "Real" ]
then
	# "SignType" will be real only for builds by scheduled and/or manual builds  of ORYX-CI
	BUILD_SIGNED="true"
else
	# locally we need to fake "binaries" directory to get a successful "copybuildscriptbinaries" build stage
    mkdir -p $BUILD_IMAGES_BUILD_CONTEXT_DIR/binaries
fi

# NOTE: We are using only one label here and put all information in it 
# in order to limit the number of layers that are created
labelContent="git_commit=$GIT_COMMIT, build_number=$BUILD_NUMBER, release_tag_name=$RELEASE_TAG_NAME"

# https://medium.com/@Drew_Stokes/bash-argument-parsing-54f3b81a6a8f
PARAMS=""
while (( "$#" )); do
  case "$1" in
    -t|--type)
      imageTypeToBuild=$2
      shift 2
      ;;
    --) # end argument parsing
      shift
      break
      ;;
    -*|--*=) # unsupported flags
      echo "Error: Unsupported flag $1" >&2
      exit 1
      ;;
    *) # preserve positional arguments
      PARAMS="$PARAMS $1"
      shift
      ;;
  esac
done
# set positional arguments in their proper place
eval set -- "$PARAMS"

echo
echo "Image type to build is set to: $imageTypeToBuild"

declare -r supportFilesImageName="support-files-image-for-build"

function BuildAndTagStage()
{
	local dockerFile="$1"
	local stageName="$2"
	local stageTagName="$ACR_PUBLIC_PREFIX/$2"

	echo
	echo "Building stage '$stageName' with tag '$stageTagName'..."
	docker build \
		--target $stageName \
		-t $stageTagName \
		-f "$dockerFile" \
		.
}

# Create artifact dir & files
mkdir -p "$ARTIFACTS_DIR/images"

touch $ACR_BUILD_IMAGES_ARTIFACTS_FILE
> $ACR_BUILD_IMAGES_ARTIFACTS_FILE

function createImageNameWithReleaseTag() {
	local imageNameToBeTaggedUniquely="$1"
	# Retag build image with build number tags
	if [ "$AGENT_BUILD" == "true" ]
	then
		IFS=':' read -ra IMAGE_NAME <<< "$imageNameToBeTaggedUniquely"
		local repo="${IMAGE_NAME[0]}"
		local tag="$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"
		if [ ${#IMAGE_NAME[@]} -eq 2 ]; then
			local uniqueImageName="$imageNameToBeTaggedUniquely-$tag"
		else
			local uniqueImageName="$repo:$tag"
		fi

		echo
		echo "Retagging image '$imageNameToBeTaggedUniquely' as '$uniqueImageName'..."
		docker tag "$imageNameToBeTaggedUniquely" "$uniqueImageName"

		# Write image list to artifacts file
		echo "$uniqueImageName" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
	fi
}

function buildGitHubRunnersBaseImage() {
	echo
	echo "----Building the image which uses GitHub runners' buildpackdeps-stretch specific digest----------"
	docker build -t githubrunners-buildpackdeps-stretch \
		-f "$BUILD_IMAGES_GITHUB_RUNNERS_BUILDPACKDEPS_STRETCH_DOCKERFILE" \
		.
}

function buildTemporaryFilesImage() {
	buildGitHubRunnersBaseImage
	
	# Create the following image so that it's contents can be copied to the rest of the images below
	echo
	echo "-------------Creating temporary files image-------------------"
	docker build -t support-files-image-for-build \
		-f "$BUILD_IMAGES_SUPPORT_FILES_DOCKERFILE" \
		.
}

function buildBuildScriptGeneratorImage() {
	buildTemporaryFilesImage

	# Create the following image so that it's contents can be copied to the rest of the images below
	echo
	echo "-------------Creating build script generator image-------------------"
	docker build -t buildscriptgenerator \
		--build-arg AGENTBUILD=$BUILD_SIGNED \
		--build-arg GIT_COMMIT=$GIT_COMMIT \
		--build-arg BUILD_NUMBER=$BUILD_NUMBER \
		--build-arg RELEASE_TAG_NAME=$RELEASE_TAG_NAME \
		-f "$BUILD_IMAGES_BUILDSCRIPTGENERATOR_DOCKERFILE" \
		.
}

function buildGitHubActionsImage() {
	buildBuildScriptGeneratorImage
	
	echo
	echo "-------------Creating build image for GitHub Actions-------------------"
	local builtImageName="$ACR_BUILD_GITHUB_ACTIONS_IMAGE_NAME"
	docker build -t $builtImageName \
		--build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$PROD_SDK_CDN_STORAGE_BASE_URL \
		--label com.microsoft.oryx="$labelContent" \
		-f "$BUILD_IMAGES_GITHUB_ACTIONS_DOCKERFILE" \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName
	echo

	docker tag $builtImageName $DEVBOX_BUILD_IMAGES_REPO:github-actions
	
	docker build \
		-t "$ORYXTESTS_BUILDIMAGE_REPO:github-actions" \
		-f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
		.
}

function buildJamStackImage() {
	buildGitHubActionsImage

	# NOTE: do not pass in label as it is inherited from base image
	# Also do not pass in build-args as they are used in base image for creating environment variables which are in
	# turn inherited by this image.
	echo
	echo "-------------Creating AzureFunctions JamStack image-------------------"
	local builtImageName="$ACR_AZURE_FUNCTIONS_JAMSTACK_IMAGE_NAME"
	docker build -t $builtImageName \
		-f "$BUILD_IMAGES_AZ_FUNCS_JAMSTACK_DOCKERFILE" \
		.
	
	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName
	echo

	docker tag $builtImageName $DEVBOX_BUILD_IMAGES_REPO:azfunc-jamstack
}

function buildLtsVersionsImage() {
	buildBuildScriptGeneratorImage
	buildGitHubRunnersBaseImage

	BuildAndTagStage "$BUILD_IMAGES_LTS_VERSIONS_DOCKERFILE" intermediate

	echo
	echo "-------------Creating lts versions build image-------------------"
	local builtImageName="$ACR_BUILD_IMAGES_REPO:lts-versions"
	docker build -t $builtImageName \
		--build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$PROD_SDK_CDN_STORAGE_BASE_URL \
		--label com.microsoft.oryx="$labelContent" \
		-f "$BUILD_IMAGES_LTS_VERSIONS_DOCKERFILE" \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName
	echo

	docker tag $builtImageName $DEVBOX_BUILD_IMAGES_REPO:lts-versions

	echo
	echo "Building a base image for tests..."
	# Do not write this image tag to the artifacts file as we do not intend to push it
	local testImageName="$ORYXTESTS_BUILDIMAGE_REPO:lts-versions"
	docker build -t $testImageName \
		-f "$ORYXTESTS_LTS_VERSIONS_BUILDIMAGE_DOCKERFILE" \
		.
}

function buildFullImage() {
	buildLtsVersionsImage

	# Pull and tag the image with the name that this image's Dockerfile expects
	local yarnImage="mcr.microsoft.com/oryx/base:build-yarn-cache-$YARN_CACHE_BASE_TAG-stretch"
	docker pull $yarnImage
	docker tag $yarnImage yarn-cache-base

	echo
	echo "-------------Creating full build image-------------------"
	local builtImageName="$ACR_BUILD_IMAGES_REPO"
	# NOTE: do not pass in label as it is inherited from base image
	# Also do not pass in build-args as they are used in base image for creating environment variables which are in
	# turn inherited by this image.
	docker build -t $builtImageName \
		-f "$BUILD_IMAGES_DOCKERFILE" \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName
	echo

	docker tag $builtImageName $DEVBOX_BUILD_IMAGES_REPO

	echo
	echo "Building a base image for tests..."
	# Do not write this image tag to the artifacts file as we do not intend to push it
	local testImageName="$ORYXTESTS_BUILDIMAGE_REPO"
	docker build -t $testImageName \
		-f "$ORYXTESTS_BUILDIMAGE_DOCKERFILE" \
		.
}

function buildVsoImage() {
	buildFullImage

	# NOTE: do not pass in label as it is inherited from base image
	# Also do not pass in build-args as they are used in base image for creating environment variables which are in
	# turn inherited by this image.
	echo
	echo "-------------Creating VSO build image-------------------"
	local builtImageName="$ACR_BUILD_VSO_IMAGE_NAME"
	docker build -t $builtImageName \
		-f "$BUILD_IMAGES_VSO_DOCKERFILE" \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName

	docker tag $builtImageName "$DEVBOX_BUILD_IMAGES_REPO:vso"

	echo
	echo "$builtImageName" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
}

function buildCliImage() {
	buildBuildScriptGeneratorImage
	
	echo
	echo "-------------Creating CLI image-------------------"
	local builtImageName="$ACR_CLI_BUILD_IMAGE_REPO"
	docker build -t $builtImageName \
		--build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$PROD_SDK_CDN_STORAGE_BASE_URL \
		--label com.microsoft.oryx="$labelContent" \
		-f "$BUILD_IMAGES_CLI_DOCKERFILE" \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName

	docker tag $builtImageName "$DEVBOX_BUILD_IMAGES_REPO:cli"

	echo
	echo "$builtImageName" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
}

function buildBuildPackImage() {
	# Build buildpack images
	# 'pack create-builder' is not supported on Windows
	if [[ "$OSTYPE" == "linux-gnu" ]] || [[ "$OSTYPE" == "darwin"* ]]; then
		source $REPO_DIR/build/buildBuildpacksImages.sh
	else
		echo
		echo "Skipping building Buildpacks images as platform '$OSTYPE' is not supported."
	fi
}

if [ -z "$imageTypeToBuild" ]; then
	buildGitHubActionsImage
	buildJamStackImage
	buildLtsVersionsImage
	buildFullImage
	buildVsoImage
	buildCliImage
	buildBuildPackImage
elif [ "$imageTypeToBuild" == "githubactions" ]; then
	buildGitHubActionsImage
elif [ "$imageTypeToBuild" == "jamstack" ]; then
	buildJamStackImage
elif [ "$imageTypeToBuild" == "ltsversions" ]; then
	buildLtsVersionsImage
elif [ "$imageTypeToBuild" == "full" ]; then
	buildFullImage
elif [ "$imageTypeToBuild" == "vso" ]; then
	buildVsoImage
elif [ "$imageTypeToBuild" == "cli" ]; then
	buildCliImage
elif [ "$imageTypeToBuild" == "buildpack" ]; then
	buildBuildPackImage
else
	echo "Error: Invalid value for '--type' switch. Valid values are: \
githubactions, jamstack, ltsversions, full, vso, cli, buildpack"
	exit 1
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
