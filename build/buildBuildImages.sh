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
echo "Image type to build is set to: $imageTypeToBuild"

if [ -z "$sdkStorageAccountUrl" ]; then
	sdkStorageAccountUrl=$PROD_SDK_CDN_STORAGE_BASE_URL
fi

# checking and retrieving token for the `oryxsdksstaging` account.
retrieveSastokenFromKeyVault $sdkStorageAccountUrl

echo
echo "SDK storage account url set to: $sdkStorageAccountUrl"

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

function buildGitHubRunnersBookwormBaseImage() {
	echo
	echo "----Building the image which uses GitHub runners' buildpackdeps-bookworm-scm specific digest----------"
	docker build -t "oryxdevmcr.azurecr.io/private/oryx/githubrunners-buildpackdeps-bookworm" \
		-f "$BUILD_IMAGES_GITHUB_RUNNERS_BUILDPACKDEPS_BOOKWORM_DOCKERFILE" \
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
	buildGitHubRunnersBookwormBaseImage

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

function buildGitHubActionsImage() {
	local debianFlavor=$1
	local devImageTag=github-actions
	local builtImageName="$ACR_BUILD_GITHUB_ACTIONS_IMAGE_NAME"

	if [ -z "$debianFlavor" ] ; then
		debianFlavor="stretch"
	fi
	devImageTag=$devImageTag-debian-$debianFlavor
	echo "dev image tag: "$devImageTag
	builtImageName=$builtImageName-debian-$debianFlavor
	echo "built image name: "$builtImageName

	buildBuildScriptGeneratorImage

	echo
	echo "-------------Creating build image for GitHub Actions-------------------"
	DOCKER_BUILDKIT=1 docker build -t $builtImageName \
		--build-arg AI_CONNECTION_STRING=$APPLICATION_INSIGHTS_CONNECTION_STRING \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$sdkStorageAccountUrl \
		--build-arg DEBIAN_FLAVOR=$debianFlavor \
		--label com.microsoft.oryx="$labelContent" \
		-f "$BUILD_IMAGES_GITHUB_ACTIONS_DOCKERFILE" \
		--secret id=oryx_sdk_storage_account_access_token,env=ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName
	echo


	docker tag $builtImageName $DEVBOX_BUILD_IMAGES_REPO:$devImageTag

	docker build \
		-t "$ORYXTESTS_BUILDIMAGE_REPO:$devImageTag" \
		--build-arg PARENT_IMAGE_BASE=$devImageTag \
		-f "$ORYXTESTS_GITHUB_ACTIONS_BUILDIMAGE_DOCKERFILE" \
		.
}

function buildJamStackImage() {
	local debianFlavor=$1
	local devImageTag=azfunc-jamstack
	local parentImageTag=cli
	local builtImageName="$ACR_AZURE_FUNCTIONS_JAMSTACK_IMAGE_NAME"

	buildCliImage $debianFlavor

	if [ -z "$debianFlavor" ]; then
		debianFlavor="stretch"
	fi
	parentImageTag=debian-$debianFlavor
	devImageTag=$devImageTag-debian-$debianFlavor
	echo "dev image tag: "$devImageTag
	builtImageName=$builtImageName-debian-$debianFlavor
	echo "built image name: "$builtImageName

	# NOTE: do not pass in label as it is inherited from base image
	# Also do not pass in build-args as they are used in base image for creating environment variables which are in
	# turn inherited by this image.
	echo
	echo "-------------Creating AzureFunctions JamStack image-------------------"
	DOCKER_BUILDKIT=1 docker build -t $builtImageName \
		-f "$BUILD_IMAGES_AZ_FUNCS_JAMSTACK_DOCKERFILE" \
		--build-arg PARENT_DEBIAN_FLAVOR=$parentImageTag \
		--build-arg DEBIAN_FLAVOR=$debianFlavor \
		--secret id=oryx_sdk_storage_account_access_token,env=ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName
	echo

	docker tag $builtImageName $DEVBOX_BUILD_IMAGES_REPO:$devImageTag
}

function buildLtsVersionsImage() {
	ltsBuildImageDockerFile=$BUILD_IMAGES_LTS_VERSIONS_DOCKERFILE
	local debianFlavor=$1
	local devImageTag=lts-versions
	local builtImageName="$ACR_BUILD_LTS_VERSIONS_IMAGE_NAME"
	local testImageName="$ORYXTESTS_BUILDIMAGE_REPO:lts-versions"

	if [ -z "$debianFlavor" ] || [ "$debianFlavor" == "stretch" ]; then
		debianFlavor="stretch"
	else
		testImageFile=$ORYXTESTS_LTS_VERSIONS_BUSTER_BUILDIMAGE_DOCKERFILE
		ltsBuildImageDockerFile=$BUILD_IMAGES_LTS_VERSIONS_BUSTER_DOCKERFILE
	fi
	testImageName=$testImageName-debian-$debianFlavor
	devImageTag=$devImageTag-debian-$debianFlavor
	builtImageName=$builtImageName-debian-$debianFlavor
	echo "dev image tag: "$devImageTag
	echo "built image name: "$builtImageName
	echo "test image name: "$testImageName

	buildBuildScriptGeneratorImage
	buildGitHubRunnersBaseImage $debianFlavor

	BuildAndTagStage "$ltsBuildImageDockerFile" intermediate

	echo
	echo "-------------Creating lts versions build image-------------------"
	DOCKER_BUILDKIT=1 docker build -t $builtImageName \
		--build-arg AI_CONNECTION_STRING=$APPLICATION_INSIGHTS_CONNECTION_STRING \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$sdkStorageAccountUrl \
		--label com.microsoft.oryx="$labelContent" \
		-f "$ltsBuildImageDockerFile" \
		--secret id=oryx_sdk_storage_account_access_token,env=ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName
	echo

	docker tag $builtImageName $DEVBOX_BUILD_IMAGES_REPO:$devImageTag

	echo
	echo "Building a base image for tests..."
	# Do not write this image tag to the artifacts file as we do not intend to push it

	docker build -t $testImageName \
		--build-arg PARENT_IMAGE_BASE=$devImageTag \
		-f "$ORYXTESTS_LTS_VERSIONS_BUILDIMAGE_DOCKERFILE" \
		.
}

function buildLatestImages() {
	local debianFlavor=$1
	if [ -z "$debianFlavor" ] ; then
		debianFlavor="stretch"
	fi
	buildLtsVersionsImage $debianFlavor

	echo
	echo "-------------Creating latest build images-------------------"
	local builtImageName="$ACR_BUILD_IMAGES_REPO:debian-$debianFlavor"
	# NOTE: do not pass in label as it is inherited from base image
	# Also do not pass in build-args as they are used in base image for creating environment variables which are in
	# turn inherited by this image.
	DOCKER_BUILDKIT=1 docker build -t $builtImageName \
		-f "$BUILD_IMAGES_DOCKERFILE" \
		--secret id=oryx_sdk_storage_account_access_token,env=ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN \
		.

	createImageNameWithReleaseTag $builtImageName

	# retag latest image with no tag so that it by default can be pulled 4 separate ways:
	# - build
	# - build:latest
	# - build:<releaseTag>
	# - build:<osType>-<osVersion>-<releaseTag>
	docker tag $builtImageName "$ACR_BUILD_IMAGES_REPO"
	createImageNameWithReleaseTag $ACR_BUILD_IMAGES_REPO

	echo
	echo "$builtImageName image history"
	docker history $builtImageName
	echo

	docker tag $builtImageName "$DEVBOX_BUILD_IMAGES_REPO:debian-$debianFlavor"

	echo
	echo "Building a base image for tests..."
	# Do not write this image tag to the artifacts file as we do not intend to push it
	local testImageName="$ORYXTESTS_BUILDIMAGE_REPO:debian-$debianFlavor"
	docker build -t $testImageName \
		-f "$ORYXTESTS_BUILDIMAGE_DOCKERFILE" \
		.
}

function buildVsoImage() {
	buildBuildScriptGeneratorImage
	buildGitHubRunnersUbuntuBaseImage
	local debianFlavor=$1
	if [ -z "$debianFlavor" ] || [ "$debianFlavor" == "focal" ]; then
		BUILD_IMAGE=$BUILD_IMAGES_VSO_FOCAL_DOCKERFILE
		local builtImageName="$ACR_BUILD_VSO_FOCAL_IMAGE_NAME"
		local tagName="vso-ubuntu-focal"
	elif [ "$debianFlavor" == "bullseye" ]; then
		BUILD_IMAGE=$BUILD_IMAGES_VSO_BULLSEYE_DOCKERFILE
		local builtImageName="$ACR_BUILD_VSO_BULLSEYE_IMAGE_NAME"
		local tagName="vso-debian-bullseye"
	else
		echo "Unsupported VSO image Debian flavor."
		exit 1
	fi

	BuildAndTagStage "$BUILD_IMAGE" intermediate
	echo
	echo "-------------Creating VSO $debianFlavor build image-------------------"

	DOCKER_BUILDKIT=1 docker build -t $builtImageName \
		--build-arg AI_CONNECTION_STRING=$APPLICATION_INSIGHTS_CONNECTION_STRING \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$sdkStorageAccountUrl \
		--label com.microsoft.oryx="$labelContent" \
		-f "$BUILD_IMAGE" \
		--secret id=oryx_sdk_storage_account_access_token,env=ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName

	docker tag $builtImageName "$DEVBOX_BUILD_IMAGES_REPO:$tagName"

	echo
	echo "$builtImageName" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
}

function buildCliImage() {
	buildBuildScriptGeneratorImage

	local debianFlavor=$1
	local devImageRepo="$DEVBOX_CLI_BUILD_IMAGE_REPO"
	local devImageTag="debian-$debianFlavor"
	local builtImageName="$ACR_CLI_BUILD_IMAGE_REPO"

	if [ -z "$debianFlavor" ] || [ $debianFlavor == "stretch" ] ; then
		debianFlavor="stretch"
		#Change buildImage name to fix validation pipeline
		builtImageName="$builtImageName:debian-$debianFlavor"
	else
		builtImageName="$builtImageName:debian-$debianFlavor"
		devImageRepo="$DEVBOX_CLI_BUILD_IMAGE_REPO-$debianFlavor"
	fi
	echo "dev image tag: "$devImageTag
	echo "built image name: "$builtImageName

	echo
	echo "-------------Creating CLI image-------------------"
	DOCKER_BUILDKIT=1 docker build -t $builtImageName \
		--build-arg AI_CONNECTION_STRING=$APPLICATION_INSIGHTS_CONNECTION_STRING \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$sdkStorageAccountUrl \
		--build-arg DEBIAN_FLAVOR=$debianFlavor \
		--label com.microsoft.oryx="$labelContent" \
		-f "$BUILD_IMAGES_CLI_DOCKERFILE" \
		--secret id=oryx_sdk_storage_account_access_token,env=ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName

	docker tag $builtImageName "$devImageRepo:$devImageTag"
}

function buildCliBuilderImage() {
	buildBuildScriptGeneratorImage
	local osType=$1
	local osFlavor=$2
	local builtImageRepo="$ACR_CLI_BUILD_IMAGE_REPO"
	local devImageRepo="$DEVBOX_CLI_BUILD_IMAGE_REPO"

	if [ -z "$osType" && -z "$osFlavor" ]; then
		osType="debian"
		osFlavor="bullseye"
	fi
	imageTag="builder-$osType-$osFlavor"
	devImageName="$devImageRepo:$imageTag"
	builtImageName="$builtImageRepo:$imageTag"

	echo
	echo "-------------Creating CLI Builder image-------------------"
	DOCKER_BUILDKIT=1 docker build -t $builtImageName \
		--build-arg AI_CONNECTION_STRING=$APPLICATION_INSIGHTS_CONNECTION_STRING \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$sdkStorageAccountUrl \
		--build-arg DEBIAN_FLAVOR=$osFlavor \
		--label com.microsoft.oryx="$labelContent" \
		-f "$BUILD_IMAGES_CLI_BUILDER_DOCKERFILE" \
		--secret id=oryx_sdk_storage_account_access_token,env=ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName

	echo "Tagging '$builtImageName' with dev name '$devImageName'"
	docker tag $builtImageName $devImageName
}

function buildFullImage() {
	buildBuildScriptGeneratorImage

	local debianFlavor=$1
	local devImageTag=full
	local builtImageName="$ACR_BUILD_FULL_IMAGE_NAME"

	if [ -z "$debianFlavor" ] ; then
		debianFlavor="stretch"
	fi
	devImageTag=$devImageTag-debian-$debianFlavor
	echo "dev image tag: "$devImageTag
	builtImageName=$builtImageName-debian-$debianFlavor
	echo "built image name: "$builtImageName

	echo
	echo "-------------Creating full image-------------------"
	DOCKER_BUILDKIT=1 docker build -t $builtImageName \
		--build-arg AI_CONNECTION_STRING=$APPLICATION_INSIGHTS_CONNECTION_STRING \
		--build-arg SDK_STORAGE_BASE_URL_VALUE=$sdkStorageAccountUrl \
		--build-arg DEBIAN_FLAVOR=$debianFlavor \
		--label com.microsoft.oryx="$labelContent" \
		-f "$BUILD_IMAGES_FULL_DOCKERFILE" \
		--secret id=oryx_sdk_storage_account_access_token,env=ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN \
		.

	createImageNameWithReleaseTag $builtImageName

	echo
	echo "$builtImageName image history"
	docker history $builtImageName

	docker tag $builtImageName "$DEVBOX_BUILD_IMAGES_REPO:$devImageTag"
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
	buildGitHubActionsImage "bookworm"
	buildGitHubActionsImage "bullseye"
	buildGitHubActionsImage "buster"
	buildGitHubActionsImage
	buildJamStackImage "bullseye"
	buildJamStackImage "buster"
	buildJamStackImage
	buildLtsVersionsImage "buster"
	buildLtsVersionsImage
	buildLatestImages
	buildVsoImage "focal"
	buildVsoImage "bullseye"
	buildCliImage "buster"
	buildCliImage "bullseye"
	buildCliImage "bookworm"
	buildCliImage
	buildCliBuilderImage "debian" "bullseye"
	buildBuildPackImage
	buildFullImage "buster"
	buildFullImage "bullseye"
elif [ "$imageTypeToBuild" == "githubactions" ]; then
	buildGitHubActionsImage
	buildGitHubActionsImage "buster"
	buildGitHubActionsImage "bullseye"
	buildGitHubActionsImage "bookworm"
elif [ "$imageTypeToBuild" == "githubactions-bookworm" ]; then
	buildGitHubActionsImage "bookworm"
elif [ "$imageTypeToBuild" == "githubactions-buster" ]; then
	buildGitHubActionsImage "buster"
elif [ "$imageTypeToBuild" == "githubactions-bullseye" ]; then
	buildGitHubActionsImage "bullseye"
elif [ "$imageTypeToBuild" == "githubactions-stretch" ]; then
	buildGitHubActionsImage
elif [ "$imageTypeToBuild" == "jamstack" ]; then
	buildJamStackImage
	buildJamStackImage "buster"
	buildJamStackImage "bullseye"
elif [ "$imageTypeToBuild" == "jamstack-bullseye" ]; then
	buildJamStackImage "bullseye"
elif [ "$imageTypeToBuild" == "jamstack-buster" ]; then
	buildJamStackImage "buster"
elif [ "$imageTypeToBuild" == "jamstack-stretch" ]; then
	buildJamStackImage
elif [ "$imageTypeToBuild" == "ltsversions" ]; then
	buildLtsVersionsImage
	buildLtsVersionsImage "buster"
elif [ "$imageTypeToBuild" == "ltsversions-buster" ]; then
	buildLtsVersionsImage "buster"
elif [ "$imageTypeToBuild" == "latest" ]; then
	buildLatestImages
elif [ "$imageTypeToBuild" == "full" ]; then
	buildFullImage "bullseye"
	buildFullImage "buster"
elif [ "$imageTypeToBuild" == "vso-focal" ]; then
	buildVsoImage "focal"
elif [ "$imageTypeToBuild" == "vso-bullseye" ]; then
	buildVsoImage "bullseye"
elif [ "$imageTypeToBuild" == "cli" ]; then
	buildCliImage
	buildCliImage "buster"
	buildCliImage "bullseye"
	buildCliImage "bookworm"
	buildCliBuilderImage "debian" "bullseye"
elif [ "$imageTypeToBuild" == "cli-stretch" ]; then
	buildCliImage
elif [ "$imageTypeToBuild" == "cli-buster" ]; then
	buildCliImage "buster"
elif [ "$imageTypeToBuild" == "cli-bullseye" ]; then
	buildCliImage "bullseye"
elif [ "$imageTypeToBuild" == "cli-bookworm" ]; then
	buildCliImage "bookworm"
elif [ "$imageTypeToBuild" == "cli-builder-bullseye" ]; then
	buildCliBuilderImage "debian" "bullseye"
elif [ "$imageTypeToBuild" == "buildpack" ]; then
	buildBuildPackImage
else
	echo "Error: Invalid value for '--type' switch. Valid values are: \
githubactions, jamstack, ltsversions, latest, full, vso-focal, cli, cli-builder-bullseye, buildpack"
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
