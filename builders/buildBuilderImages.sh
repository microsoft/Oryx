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
source $REPO_DIR/build/__nodeVersions.sh

cd "$BUILD_IMAGES_BUILD_CONTEXT_DIR"

declare BUILD_SIGNED=""

# Check to see if the build is by scheduled ORYX-CI or other azure devops build
# SIGNTYPE is set to 'real' on the Oryx-CI build definition itself (not in yaml file)
if [ "$SIGNTYPE" == "real" ] || [ "$SIGNTYPE" == "Real" ]
then
	# "SignType" will be real only for builds by scheduled and/or manual builds of ORYX-CI
	BUILD_SIGNED="true"
else
	# locally we need to fake "binaries" directory to get a successful "copybuildscriptbinaries" build stage
	mkdir -p $BUILD_IMAGES_BUILD_CONTEXT_DIR/binaries
fi

# NOTE: We are using only one label here and put all information in it 
# in order to limit the number of layers that are created
labelContent="git_commit=$GIT_COMMIT, build_number=$BUILD_NUMBER, release_tag_name=$RELEASE_TAG_NAME"

# default parameter values
imageTypeToBuild=""
sdkStorageAccountUrl=$PROD_SDK_CDN_STORAGE_BASE_URL

# https://medium.com/@Drew_Stokes/bash-argument-parsing-54f3b81a6a8f
PARAMS=""
while (( "$#" )); do
	case "$1" in
		-t|--type)
		imageTypeToBuild=$2
		shift 2
		;;
		-s|--sdk-storage-account-url)
		sdkStorageAccountUrl=$2
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
echo "Image type: $imageTypeToBuild"
echo "SDK storage account url: $sdkStorageAccountUrl"

declare -r supportFilesImageName="oryxdevmcr.azurecr.io/private/oryx/support-files-image-for-build"

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
	else
		# Write non-ci image to artifacts file as is, for local testing
		echo "$imageNameToBeTaggedUniquely" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
	fi
}

function buildGitHubRunnersUbuntuBaseImage() {
	
	echo
	echo "----Building the image which uses GitHub runners' buildpackdeps-focal-scm specific digest----------"
	docker build -t "oryxdevmcr.azurecr.io/private/oryx/githubrunners-buildpackdeps-focal" \
		-f "$BUILD_IMAGES_GITHUB_RUNNERS_BUILDPACKDEPS_FOCAL_DOCKERFILE" \
		.
}

function buildGitHubRunnersBullseyeBaseImage() {
	echo
	echo "----Building the image which uses GitHub runners' buildpackdeps-bullseye-scm specific digest----------"
	docker build -t "oryxdevmcr.azurecr.io/private/oryx/githubrunners-buildpackdeps-bullseye" \
		-f "$BUILD_IMAGES_GITHUB_RUNNERS_BUILDPACKDEPS_BULLSEYE_DOCKERFILE" \
		.
}

function buildGitHubRunnersBusterBaseImage() {
	
	echo
	echo "----Building the image which uses GitHub runners' buildpackdeps-buster-scm specific digest----------"
	docker build -t "oryxdevmcr.azurecr.io/private/oryx/githubrunners-buildpackdeps-buster" \
		-f "$BUILD_IMAGES_GITHUB_RUNNERS_BUILDPACKDEPS_BUSTER_DOCKERFILE" \
		.
}

function buildGitHubRunnersBaseImage() {
	echo
	echo "----Building the image which uses GitHub runners' buildpackdeps-stretch specific digest----------"
	docker build -t "oryxdevmcr.azurecr.io/private/oryx/githubrunners-buildpackdeps-stretch" \
		-f "$BUILD_IMAGES_GITHUB_RUNNERS_BUILDPACKDEPS_STRETCH_DOCKERFILE" \
		.
}

function buildTemporaryFilesImage() {
	buildGitHubRunnersBaseImage
	buildGitHubRunnersBusterBaseImage
	buildGitHubRunnersUbuntuBaseImage
	buildGitHubRunnersBullseyeBaseImage

	# Create the following image so that it's contents can be copied to the rest of the images below
	echo
	echo "------Creating temporary files image-------"
	docker build -t "oryxdevmcr.azurecr.io/private/oryx/support-files-image-for-build" \
		-f "$BUILD_IMAGES_SUPPORT_FILES_DOCKERFILE" \
		.
}

function buildBuildScriptGeneratorImage() {
	buildTemporaryFilesImage

	# Create the following image so that it's contents can be copied to the rest of the images below
	echo
	echo "-------------Creating build script generator image-------------------"
	docker build -t "oryxdevmcr.azurecr.io/private/oryx/buildscriptgenerator" \
		--build-arg AGENTBUILD=$BUILD_SIGNED \
		--build-arg GIT_COMMIT=$GIT_COMMIT \
		--build-arg BUILD_NUMBER=$BUILD_NUMBER \
		--build-arg RELEASE_TAG_NAME=$RELEASE_TAG_NAME \
		-f "$BUILD_IMAGES_BUILDSCRIPTGENERATOR_DOCKERFILE" \
		.
}

function buildBaseBuilderImage() {
	buildBuildScriptGeneratorImage
	
	local debianFlavor=$1
	local builtImageRepo="$ACR_CLI_BUILD_IMAGE_REPO"
	local devImageRepo="$DEVBOX_CLI_BUILD_IMAGE_REPO"

	if [ -z "$debianFlavor" ]; then
		debianFlavor="stretch"
	fi
	imageTag="builder-debian-$debianFlavor"
	devImageName="$devImageRepo:$imageTag"
	builtImageName="$builtImageRepo:$imageTag"

	echo
	echo "-------------Creating CLI Builder image-------------------"
	docker build -t $builtImageName \
		--build-arg AI_CONNECTION_STRING=$APPLICATION_INSIGHTS_CONNECTION_STRING \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$sdkStorageAccountUrl \
		--build-arg DEBIAN_FLAVOR=$debianFlavor \
		--label com.microsoft.oryx="$labelContent" \
		-f "$BUILD_IMAGES_CLI_BUILDER_DOCKERFILE" \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName

	echo "Tagging '$builtImageName' with dev name '$devImageName'"
	docker tag $builtImageName $devImageName
}

function buildContainerAppsBuilderImage() {
	buildBuildScriptGeneratorImage
	
	local debianFlavor=$1
	local builtImageRepo="$ACR_CLI_BUILD_IMAGE_REPO"
	local devImageRepo="$DEVBOX_CLI_BUILD_IMAGE_REPO"

	if [ -z "$debianFlavor" ]; then
		debianFlavor="stretch"
	fi
	imageTag="builder-debian-$debianFlavor"
	devImageName="$devImageRepo:$imageTag"
	builtImageName="$builtImageRepo:$imageTag"

	echo
	echo "-------------Creating CLI Builder image-------------------"
	docker build -t $builtImageName \
		--build-arg AI_CONNECTION_STRING=$APPLICATION_INSIGHTS_CONNECTION_STRING \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$sdkStorageAccountUrl \
		--build-arg DEBIAN_FLAVOR=$debianFlavor \
		--label com.microsoft.oryx="$labelContent" \
		-f "$BUILD_IMAGES_CLI_BUILDER_DOCKERFILE" \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName

	echo "Tagging '$builtImageName' with dev name '$devImageName'"
	docker tag $builtImageName $devImageName
}

if [ -z "$imageTypeToBuild" ]; then
	buildBaseBuilderImage
	buildContainerAppsBuilderImage
elif [ "$imageTypeToBuild" == "base" ]; then
	buildBaseBuilderImage
elif [ "$imageTypeToBuild" == "capps" || "$imageTypeToBuild" == "container-apps" ]; then
	buildGitHubActionsImage "buster"
else
	echo "Error: Invalid value for '--type' switch. Valid values are: \
base, capps, container-apps"
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
