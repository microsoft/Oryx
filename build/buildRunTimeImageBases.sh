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
source $REPO_DIR/build/__nodeVersions.sh
source $REPO_DIR/build/__stagingRuntimeConstants.sh

declare -r NODE_BOOKWORM_VERSION_ARRAY=()
declare -r NODE_BULLSEYE_VERSION_ARRAY=($NODE18_VERSION $NODE16_VERSION $NODE14_VERSION)
declare -r NODE_BUSTER_VERSION_ARRAY=($NODE16_VERSION $NODE14_VERSION)

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

if [ ! -z "$runtimeSubDir" ]
then
    runtimeImagesSourceDir="$runtimeImagesSourceDir/$runtimeSubDir"
    if [ ! -d "$runtimeImagesSourceDir" ]; then
        (>&2 echo "Unknown runtime '$runtimeSubDir'")
        exit 1
    fi
fi

# checking and retrieving token for the `oryxsdksstaging` account.
retrieveSastokenFromKeyVault $PRIVATE_STAGING_SDK_STORAGE_BASE_URL

echo
echo "Building the common base image wih bullseye, buster, and bookworm flavor '$RUNTIME_BASE_IMAGE_NAME'..."
echo
# Build the common base image first, so other images that depend on it get the latest version.
# We don't retrieve this image from a repository but rather build locally to make sure we get
# the latest version of its own base image.
docker build \
    --pull \
    --build-arg DEBIAN_FLAVOR=buster \
    -f "$RUNTIME_BASE_IMAGE_DOCKERFILE_PATH" \
    -t "oryxdevmcr.azurecr.io/private/oryx/$RUNTIME_BASE_IMAGE_NAME-buster" \
    $REPO_DIR

docker build \
    --pull \
    --build-arg DEBIAN_FLAVOR=bullseye \
    -f "$RUNTIME_BASE_IMAGE_DOCKERFILE_PATH" \
    -t "oryxdevmcr.azurecr.io/private/oryx/$RUNTIME_BASE_IMAGE_NAME-bullseye" \
    $REPO_DIR

docker build \
    --pull \
    --build-arg DEBIAN_FLAVOR=bookworm \
    -f "$RUNTIME_BASE_IMAGE_DOCKERFILE_PATH" \
    -t "oryxdevmcr.azurecr.io/private/oryx/$RUNTIME_BASE_IMAGE_NAME-bookworm" \
    $REPO_DIR

labels="--label com.microsoft.oryx.git-commit=$GIT_COMMIT"
labels="$labels --label com.microsoft.oryx.build-number=$BUILD_NUMBER"

execAllGenerateDockerfiles "$runtimeImagesSourceDir" "generateDockerfiles.sh" "$runtimeImageDebianFlavor"

dockerFileName="base.$runtimeImageDebianFlavor.Dockerfile"
stagingDockerFileName="base.$runtimeImageDebianFlavor.staging.Dockerfile"
dockerFiles=$(find $runtimeImagesSourceDir -type f \( -name $dockerFileName -o -name $stagingDockerFileName \))

nodeDockerfiles=()

if [ "$runtimeSubDir" == "node" ]; then
    docker build \
        --build-arg DEBIAN_FLAVOR=$runtimeImageDebianFlavor \
        -f "$REPO_DIR/images/runtime/commonbase/nodeRuntimeBase.Dockerfile" \
        -t "oryxdevmcr.azurecr.io/private/oryx/oryx-node-run-base-$runtimeImageDebianFlavor" \
        $REPO_DIR

    NODE_VERSION_ARRAY=()
    if [ "$runtimeImageDebianFlavor" == "bookworm" ];then
        NODE_VERSION_ARRAY=("${NODE_BOOKWORM_VERSION_ARRAY[@]}")
    elif [ "$runtimeImageDebianFlavor" == "bullseye" ];then
        NODE_VERSION_ARRAY=("${NODE_BULLSEYE_VERSION_ARRAY[@]}")
    elif [ "$runtimeImageDebianFlavor" == "buster" ]; then
        NODE_VERSION_ARRAY=("${NODE_BUSTER_VERSION_ARRAY[@]}")
    fi

    for NODE_VERSION  in "${NODE_VERSION_ARRAY[@]}"
    do
        IFS='.' read -ra SPLIT_VERSION <<< "$NODE_VERSION"
        VERSION_DIRECTORY="${SPLIT_VERSION[0]}"
        eachFile=$runtimeImagesSourceDir/$VERSION_DIRECTORY/$dockerFileName
        nodeDockerfiles+=( "$eachFile" )
    done
    dockerFiles="${nodeDockerfiles[@]}"
fi

# Write the list of images that were built to artifacts folder
mkdir -p "$BASE_IMAGES_ARTIFACTS_FILE_PREFIX"

# NOTE: We create a unique artifacts file per platform since they are going to be built in parallel on CI
ARTIFACTS_FILE="$BASE_IMAGES_ARTIFACTS_FILE_PREFIX/$runtimeSubDir-runtimeimage-bases-$runtimeImageDebianFlavor.txt"

initFile="$runtimeImagesSourceDir/buildRunTimeImageBases_Init.sh"
if [ -f "$initFile" ]; then
    $initFile $runtimeImageDebianFlavor
fi

if [ -z "$dockerFiles" ]
then
    echo "Couldn't find any Dockerfiles with name '$dockerFileName' under '$runtimeImagesSourceDir' and its sub-directories."
    exit 1
fi

clearedOutput=false
for dockerFile in $dockerFiles; do
    dockerFileDir=$(dirname "${dockerFile}")

    # Set $getTagName_result to the following format: {platformName}:{platformVersion}
    getTagName $dockerFileDir

    IFS=':' read -ra PARTS <<< "$getTagName_result"
    platformName="${PARTS[0]}"
    platformVersion="${PARTS[1]}"

    if shouldStageRuntimeVersion $platformName $platformVersion ; then
        # skip the normal base.{ostype}.Dockerfile if this version should be staged
        if [[ "$dockerFile" != *"staging"* ]]; then
            continue
        fi
        # Set $localImageTagName to the following format: oryxdevmcr.azurecr.io/staging/oryx/base:{platformName}-{platformVersion}-{osType}
        localImageTagName="$BASE_IMAGES_STAGING_REPO:$platformName-$platformVersion-debian-$runtimeImageDebianFlavor"
    else
        # skip the base.{ostype}.staging.Dockerfile if this version should not be staged
        if [[ "$dockerFile" == *"staging"* ]]; then
            continue
        fi
        # Set $localImageTagName to the following format: oryxdevmcr.azurecr.io/public/oryx/base:{platformName}-{platformVersion}-{osType}
        localImageTagName="$BASE_IMAGES_PUBLIC_REPO:$platformName-$platformVersion-debian-$runtimeImageDebianFlavor"
    fi

    echo
    echo "Building image '$localImageTagName' for Dockerfile located at '$dockerFile'..."

    cd $REPO_DIR

    echo

    # pass in env var as a secret, which is mounted during a single run command of the build
    # https://github.com/docker/buildx/blob/master/docs/reference/buildx_build.md#secret
    DOCKER_BUILDKIT=1 docker build -f $dockerFile \
        -t $localImageTagName \
        --build-arg CACHEBUST=$(date +%s) \
        --build-arg NODE14_VERSION=$NODE14_VERSION \
        --build-arg NODE16_VERSION=$NODE16_VERSION \
        --build-arg NODE18_VERSION=$NODE18_VERSION \
        --build-arg DEBIAN_FLAVOR=$runtimeImageDebianFlavor \
        --secret id=oryx_sdk_storage_account_access_token,env=ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN \
        --secret id=dotnet_storage_account_token_id,env=DOTNET_PRIVATE_STORAGE_ACCOUNT_ACCESS_TOKEN \
        $labels \
        .


    # Retag build image with build numbers as ACR tags
    if [ "$AGENT_BUILD" == "true" ]
    then
        # $tag will follow a similar format to 20191024.1
        uniqueImageName="$localImageTagName-$BUILD_NUMBER"

        # Tag the image to follow a similar format to .../python:3.7-debian-bullseye-20191028.1
        docker tag "$localImageTagName" "$uniqueImageName"

        if [ $clearedOutput == "false" ]
        then
            # clear existing contents of the file, if any
            > $ARTIFACTS_FILE
            clearedOutput=true
        fi

        # add new content
        echo
        echo "Updating artifacts file with the built runtime image information..."
        echo "$uniqueImageName" >> $ARTIFACTS_FILE
    fi

    cd $RUNTIME_IMAGES_SRC_DIR
done

if [ "$AGENT_BUILD" == "true" ]
then
    echo
    echo "List of images tagged (from '$ARTIFACTS_FILE'):"
    cat $ARTIFACTS_FILE
fi

echo
showDockerImageSizes

echo
dockerCleanupIfRequested