#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

# Load all variables
source $REPO_DIR/build/__extVarNames.sh
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__functions.sh
source $REPO_DIR/build/__sdkStorageConstants.sh
source $REPO_DIR/build/__stagingRuntimeConstants.sh

# https://medium.com/@Drew_Stokes/bash-argument-parsing-54f3b81a6a8f
PARAMS=""
while (( "$#" )); do
  case "$1" in
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

if [ -z "$sdkStorageAccountUrl" ]; then
  sdkStorageAccountUrl=$PROD_SDK_CDN_STORAGE_BASE_URL
fi

# checking and retrieving token for the `oryxsdksstaging` account.
retrieveSastokenFromKeyVault $sdkStorageAccountUrl

echo
echo "SDK storage account url set to: $sdkStorageAccountUrl"

runtimeImagesSourceDir="$RUNTIME_IMAGES_SRC_DIR"
runtimeSubDir=""
runtimeImageDebianFlavor="buster"

if [ $# -eq 2 ] 
then
    echo "Locally building runtime '$runtimeSubDir'"
    runtimeSubDir="$1"
    runtimeImageDebianFlavor="$2"
elif [ $# -eq 1 ]
then
    echo "CI Agent building runtime '$runtimeSubDir'"
    runtimeImageDebianFlavor="$1"
fi

echo "Setting environment variable 'ORYX_RUNTIME_DEBIAN_FLAVOR' to provided value '$runtimeImageDebianFlavor'."
export ORYX_RUNTIME_DEBIAN_FLAVOR="$runtimeImageDebianFlavor"

if [ ! -z "$runtimeSubDir" ]
then
    runtimeImagesSourceDir="$runtimeImagesSourceDir/$runtimeSubDir"
    if [ ! -d "$runtimeImagesSourceDir" ]; then
        (>&2 echo "Unknown runtime '$runtimeSubDir'")
        exit 1
    fi
fi

labels="--label com.microsoft.oryx.git-commit=$GIT_COMMIT"
labels="$labels --label com.microsoft.oryx.build-number=$BUILD_NUMBER"
labels="$labels --label com.microsoft.oryx.release-tag-name=$RELEASE_TAG_NAME"

# Avoid causing cache invalidation with the following check
if [ "$EMBED_BUILDCONTEXT_IN_IMAGES" == "true" ]
then
    args="--build-arg GIT_COMMIT=$GIT_COMMIT"
    args="$args --build-arg BUILD_NUMBER=$BUILD_NUMBER"
    args="$args --build-arg RELEASE_TAG_NAME=$RELEASE_TAG_NAME"
fi

# Build the common base image first, so other images that depend on it get the latest version.
# We don't retrieve this image from a repository but rather build locally to make sure we get
# the latest version of its own base image.

docker build \
    --pull \
    -f "$RUNTIME_BASE_IMAGE_DOCKERFILE_PATH" \
    -t "oryxdevmcr.azurecr.io/private/oryx/$RUNTIME_BASE_IMAGE_NAME-buster" \
    --build-arg DEBIAN_FLAVOR=buster \
    $REPO_DIR

docker build \
    --pull \
    -f "$RUNTIME_BASE_IMAGE_DOCKERFILE_PATH" \
    -t "oryxdevmcr.azurecr.io/private/oryx/$RUNTIME_BASE_IMAGE_NAME-bullseye" \
    --build-arg DEBIAN_FLAVOR=bullseye \
    $REPO_DIR

docker build \
    --pull \
    -f "$RUNTIME_BASE_IMAGE_DOCKERFILE_PATH" \
    -t "oryxdevmcr.azurecr.io/private/oryx/$RUNTIME_BASE_IMAGE_NAME-bookworm" \
    --build-arg DEBIAN_FLAVOR=bookworm \
    $REPO_DIR

execAllGenerateDockerfiles "$runtimeImagesSourceDir" "generateDockerfiles.sh" "$runtimeImageDebianFlavor"

# The common base image is built separately, so we ignore it
dockerFiles=$(find $runtimeImagesSourceDir -type f \( -name "$runtimeImageDebianFlavor.Dockerfile" ! -path "$RUNTIME_IMAGES_SRC_DIR/commonbase/*" \) )
if [ -z "$dockerFiles" ]
then
    echo "Couldn't find any Dockerfiles under '$runtimeImagesSourceDir' and its sub-directories."
    exit 1
fi

# Write the list of images that were built to artifacts folder
mkdir -p "$ARTIFACTS_DIR/images"

if [ "$AGENT_BUILD" == "true" ]
then
    # clear existing contents of the file, if any
    > $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE.$runtimeImageDebianFlavor.txt
fi

for dockerFile in $dockerFiles; do
    dockerFileDir=$(dirname "${dockerFile}")

    # get tag name without os type first, so we can extract platform name and version
    getTagName $dockerFileDir
    IFS=':' read -ra PARTS <<< "$getTagName_result"
    platformName="${PARTS[0]}"
    platformVersion="${PARTS[1]}"

    # Set $getTagName_result to the following format: {platformName}:{platformVersion}-{osType}
    getTagName $dockerFileDir debian-$runtimeImageDebianFlavor

    if shouldStageRuntimeVersion $platformName $platformVersion ; then
        # Set $localImageTagName to the following format: oryxdevmcr.azurecr.io/staging/oryx/{platformName}:{platformVersion}-{osType}
        localImageTagName="$ACR_STAGING_PREFIX/$getTagName_result"
    else
        # Set $localImageTagName to the following format: oryxdevmcr.azurecr.io/public/oryx/{platformName}:{platformVersion}-{osType}
        localImageTagName="$ACR_PUBLIC_PREFIX/$getTagName_result"
    fi

    echo
    echo "Building image '$localImageTagName' for Dockerfile located at '$dockerFile'..."

    cd $REPO_DIR
    
    echo
    
    # pass in env var as a secret, which is mounted during a single run command of the build
    # https://github.com/docker/buildx/blob/master/docs/reference/buildx_build.md#secret
    DOCKER_BUILDKIT=1 docker build -f $dockerFile \
        -t $localImageTagName \
        --build-arg AI_CONNECTION_STRING=$APPLICATION_INSIGHTS_CONNECTION_STRING \
        --build-arg SDK_STORAGE_ENV_NAME=$SDK_STORAGE_BASE_URL_KEY_NAME \
        --build-arg SDK_STORAGE_BASE_URL_VALUE=$sdkStorageAccountUrl \
        --build-arg DEBIAN_FLAVOR=$runtimeImageDebianFlavor \
        --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION \
        --secret id=oryx_sdk_storage_account_access_token,env=ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN \
        $args \
        $labels \
        .

    echo
    echo "'$localImageTagName' image history:"
    docker history $localImageTagName
    echo

    echo "$localImageTagName" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE.$runtimeImageDebianFlavor.txt

    # Retag image with build number (for images built in buildAgent)
    if [ "$AGENT_BUILD" == "true" ]
    then
        # $uniqueTag will follow a similar format to Oryx-CI.20191028.1
        # $BUILD_DEFINITIONNAME is the name of the build (e.g., Oryx-CI)
        # $RELEASE_TAG_NAME is either the date of the build if the branch is master/main, or
        # the name of the branch the build is against
        uniqueTag="$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"

        if shouldStageRuntimeVersion $platformName $platformVersion ; then
            # Set $acrRuntimeImageTagNameRepo to the following format: oryxdevmcr.azurecr.io/staging/oryx/{platformName}:{platformVersion}
            acrRuntimeImageTagNameRepo="$ACR_STAGING_PREFIX/$getTagName_result"
        else
            # Set $acrRuntimeImageTagNameRepo to the following format: oryxdevmcr.azurecr.io/public/oryx/{platformName}:{platformVersion}
            acrRuntimeImageTagNameRepo="$ACR_PUBLIC_PREFIX/$getTagName_result"
        fi

        # Tag the image to follow a similar format to .../python:3.7-Oryx-CI.20191028.1
        docker tag "$localImageTagName" "$acrRuntimeImageTagNameRepo-$uniqueTag"

        # add new content
        echo
        echo "Updating runtime image artifacts file with build number..."
        echo "$acrRuntimeImageTagNameRepo-$uniqueTag" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE.$runtimeImageDebianFlavor.txt
    else
        devBoxRuntimeImageTagNameRepo="$DEVBOX_RUNTIME_IMAGES_REPO_PREFIX/$getTagName_result"
        docker tag "$localImageTagName" "$devBoxRuntimeImageTagNameRepo"
    fi

    cd $RUNTIME_IMAGES_SRC_DIR
done

if [ "$AGENT_BUILD" == "true" ]
then
    echo
    echo "List of images tagged (from '$ACR_RUNTIME_IMAGES_ARTIFACTS_FILE.$runtimeImageDebianFlavor.txt'):"
    cat $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE.$runtimeImageDebianFlavor.txt
fi

echo
showDockerImageSizes

echo
dockerCleanupIfRequested
