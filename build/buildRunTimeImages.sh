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

# Load platform versions for runtimes
source $REPO_DIR/build/__dotNetCoreRunTimeVersions.sh
source $REPO_DIR/build/__nodeVersions.sh
source $REPO_DIR/build/__phpVersions.sh
source $REPO_DIR/build/__pythonVersions.sh
source $REPO_DIR/build/__rubyVersions.sh

# Get the specific platform version used for each runtime image to assist with future patching
# e.g., for dotnetcore:7.0-debian-buster, we would retrieve 7.0.10 from the __dotNetCoreRunTimeVersions.sh file
function getRuntimeTagVersion()
{
    if [ -z $1 ]
    then
        echo "Runtime platform name was not provided"
        return 1
    fi

    if [ -z $2 ]
    then
        echo "Runtime platform version was not provided"
        return 1
    fi

    PLATFORM_NAME=$1
    PLATFORM_VERSION=$2

    if [ "$PLATFORM_NAME" == "dotnetcore" ]
    then
        case $PLATFORM_VERSION in

            8.0)
                FULL_RUNTIME_TAG_VERSION=$NET_CORE_APP_80
                ;;
            7.0)
                FULL_RUNTIME_TAG_VERSION=$NET_CORE_APP_70
                ;;
            6.0)
                FULL_RUNTIME_TAG_VERSION=$NET_CORE_APP_60
                ;;
            5.0)
                FULL_RUNTIME_TAG_VERSION=$NET_CORE_APP_50
                ;;
            3.1)
                FULL_RUNTIME_TAG_VERSION=$NET_CORE_APP_31
                ;;
            3.0)
                FULL_RUNTIME_TAG_VERSION=$NET_CORE_APP_30
                ;;
            *)
                FULL_RUNTIME_TAG_VERSION=$PLATFORM_VERSION
                ;;
        esac
    elif [ "$PLATFORM_NAME" == "node" ]
    then
        case $PLATFORM_VERSION in

            18)
                FULL_RUNTIME_TAG_VERSION=$NODE18_VERSION
                ;;
            16)
                FULL_RUNTIME_TAG_VERSION=$NODE16_VERSION
                ;;
            14)
                FULL_RUNTIME_TAG_VERSION=$NODE14_VERSION
                ;;
            *)
                FULL_RUNTIME_TAG_VERSION=$PLATFORM_VERSION
                ;;
        esac
    elif [ "$PLATFORM_NAME" == "php" ]
    then
        case $PLATFORM_VERSION in
            8.2)
                FULL_RUNTIME_TAG_VERSION=$PHP82_VERSION
                ;;
            8.1)
                FULL_RUNTIME_TAG_VERSION=$PHP81_VERSION
                ;;
            8.0)
                FULL_RUNTIME_TAG_VERSION=$PHP80_VERSION
                ;;
            7.4)
                FULL_RUNTIME_TAG_VERSION=$PHP80_VERSION
                ;;
            *)
                FULL_RUNTIME_TAG_VERSION=$PLATFORM_VERSION
                ;;
        esac
    elif [ "$PLATFORM_NAME" == "python" ]
    then
        case $PLATFORM_VERSION in
            3.11)
                FULL_RUNTIME_TAG_VERSION=$PYTHON311_VERSION
                ;;
            3.10)
                FULL_RUNTIME_TAG_VERSION=$PYTHON310_VERSION
                ;;
            3.9)
                FULL_RUNTIME_TAG_VERSION=$PYTHON39_VERSION
                ;;
            3.8)
                FULL_RUNTIME_TAG_VERSION=$PYTHON38_VERSION
                ;;
            3.7)
                FULL_RUNTIME_TAG_VERSION=$PYTHON37_VERSION
                ;;
            *)
                FULL_RUNTIME_TAG_VERSION=$PLATFORM_VERSION
                ;;
        esac
    elif [ "$PLATFORM_NAME" == "ruby" ]
    then
        case $PLATFORM_VERSION in
            2.7)
                FULL_RUNTIME_TAG_VERSION=$RUBY27_VERSION
                ;;
            2.6)
                FULL_RUNTIME_TAG_VERSION=$RUBY26_VERSION
                ;;
            2.5)
                FULL_RUNTIME_TAG_VERSION=$RUBY25_VERSION
                ;;
            *)
                FULL_RUNTIME_TAG_VERSION=$PLATFORM_VERSION
                ;;
        esac
    else
        echo "Unable to retrieve version from the provided runtime platform name '$PLATFORM_NAME'"
        return 1
    fi

    return 0
}

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

    # Get the full platform version for an alternative tag (used for patching)
    getRuntimeTagVersion $platformName $platformVersion

    if shouldStageRuntimeVersion $platformName $platformVersion ; then
        # Set $localImageTagName to the following format: oryxdevmcr.azurecr.io/staging/oryx/{platformName}:{platformVersion}-{osType}
        localImageTagName="$ACR_STAGING_PREFIX/$platformName:$platformVersion-debian-$runtimeImageDebianFlavor"
    else
        # Set $localImageTagName to the following format: oryxdevmcr.azurecr.io/public/oryx/{platformName}:{platformVersion}-{osType}
        localImageTagName="$ACR_PUBLIC_PREFIX/$platformName:$platformVersion-debian-$runtimeImageDebianFlavor"
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

    # Retag to include the full runtime platform version found (for patching)
    if [ "$FULL_RUNTIME_TAG_VERSION" != "$platformVersion" ]; then
        if shouldStageRuntimeVersion $platformName $platformVersion ; then
            # Set $altLocalImageTagName to the following format: oryxdevmcr.azurecr.io/staging/oryx/{platformName}:{fullPlatformVersion}-{osType}
            altLocalImageTagName="$ACR_STAGING_PREFIX/$platformName:$FULL_RUNTIME_TAG_VERSION-debian-$runtimeImageDebianFlavor"
        else
            # Set $altLocalImageTagName to the following format: oryxdevmcr.azurecr.io/public/oryx/{platformName}:{fullPlatformVersion}-{osType}
            altLocalImageTagName="$ACR_PUBLIC_PREFIX/$platformName:$FULL_RUNTIME_TAG_VERSION-debian-$runtimeImageDebianFlavor"
        fi

        echo
        echo "Tagging image '$localImageTagName' with alternative tag '$altLocalImageTagName' to include full platform version..."
        docker tag "$localImageTagName" "$altLocalImageTagName"
        echo "$altLocalImageTagName" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE.$runtimeImageDebianFlavor.txt
    fi

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

        # Tag the image to follow a similar format to .../python:3.8-debian-bullseye-Oryx-CI.20230828.1
        acrRuntimeImageUniqueTag="$acrRuntimeImageTagNameRepo-debian-$runtimeImageDebianFlavor-$uniqueTag"
        docker tag "$localImageTagName" "$acrRuntimeImageUniqueTag"

        # add new content
        echo
        echo "Updating runtime image artifacts file with build number..."
        echo "$acrRuntimeImageUniqueTag" >> $ACR_RUNTIME_IMAGES_ARTIFACTS_FILE.$runtimeImageDebianFlavor.txt
    else
        devBoxRuntimeImageTagNameRepo="$DEVBOX_RUNTIME_IMAGES_REPO_PREFIX/$getTagName_result"
        docker tag "$localImageTagName" "$devBoxRuntimeImageTagNameRepo-debian-$runtimeImageDebianFlavor"
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
