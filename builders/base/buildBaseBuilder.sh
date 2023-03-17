#!/bin/bash
set -ex

declare -r SCRIPT_DIR=$( cd $( dirname "$0" ) && pwd )
declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/build/__variables.sh
source $REPO_DIR/build/__sdkStorageConstants.sh

# constants
declare -r ORYX_AI_CONNECTION_STRING_PLACEHOLDER="%ORYX_AI_CONNECTION_STRING%"
declare -r ORYX_SDK_STORAGE_BASE_URL_PLACEHOLDER="%ORYX_SDK_STORAGE_BASE_URL%"
declare -r ORYX_BUILDPACK_IMAGE_PLACEHOLDER="%ORYX_BUILDPACK_IMAGE%"
declare -r ORYX_BUILDPACK_VERSION_PLACEHOLDER="%ORYX_BUILDPACK_VERSION%"
declare -r ORYX_RUN_STACK_IMAGE_PLACEHOLDER="%ORYX_RUN_STACK_IMAGE%"
declare -r ORYX_BUILD_STACK_IMAGE_PLACEHOLDER="%ORYX_BUILD_STACK_IMAGE%"
declare -r MCR_BUILDER_IMAGE_REPO="mcr.microsoft.com/oryx/builder"
declare -r MCR_CLI_IMAGE_REPO="mcr.microsoft.com/oryx/cli"

# parameter defaults
builderImageVersion="20230208.1"
destinationFqdn="oryxprodmcr.azurecr.io"
destinationRepo="public/oryx/builder"
buildpackVersion="0.0.4"
storageAccountUrl="$PROD_SDK_CDN_STORAGE_BASE_URL"

PARAMS=""
while (( "$#" )); do
  case "$1" in
    -c|--cli-builder-image)
    cliBuilderImage=$2
    shift 2
    ;;
    -v|--builder-image-version)
    builderImageVersion=$2
    shift 2
    ;;
    -f|--destination-registry-fqdn)
    destinationFqdn=$2
    shift 2
    ;;
    -r|--destination-registry-repo)
    destinationRepo=$2
    shift 2
    ;;
    --buildpack-version)
    buildpackVersion=$2
    shift 2
    ;;
    -s|--storage-account-url)
    storageAccountUrl=$2
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

function replaceRepo() {
  local imageName="$1"
  local newRepo="$2"
  # Retag build image with new repo
    IFS=':' read -ra SPLIT_IMAGE_NAME <<< "$imageName"
    local repo="${SPLIT_IMAGE_NAME[0]}"
    local tag="${SPLIT_IMAGE_NAME[1]}"
    local newImage="$newRepo:$tag"
    docker tag "$imageName" "$newImage"
    echo $newImage
}

if [ -z $cliBuilderImage ]; then
    cliBuilderImage="$destinationFqdn/public/oryx/cli:builder-debian-buster-$builderImageVersion"
    docker pull $cliBuilderImage
    mcrCliBuilderImage=$(replaceRepo "$cliBuilderImage" "$MCR_CLI_IMAGE_REPO")
fi

# Create artifact dir & files
echo "Initializing artifacts file: $ACR_BUILDER_IMAGES_ARTIFACTS_FILE"
mkdir -p "$ARTIFACTS_DIR/images"
touch $ACR_BUILDER_IMAGES_ARTIFACTS_FILE
> $ACR_BUILDER_IMAGES_ARTIFACTS_FILE

# Building stack
echo "Tagging all images with the tag: $builderImageVersion"
echo "-------------------------------------------------"
echo
echo "Building stack..."
echo
baseImage="$destinationFqdn/$destinationRepo:stack-base-$builderImageVersion"
buildStackImage="$destinationFqdn/$destinationRepo:stack-build-$builderImageVersion"
runStackImage="$destinationFqdn/$destinationRepo:stack-run-$builderImageVersion"

docker build $SCRIPT_DIR/stack/ \
    --build-arg CLI_BUILDER_IMAGE="$mcrCliBuilderImage" \
    -t $baseImage \
    --target base

docker build $SCRIPT_DIR/stack/ \
    --build-arg CLI_BUILDER_IMAGE="$mcrCliBuilderImage" \
    -t $runStackImage \
    --target run

docker build $SCRIPT_DIR/stack/  \
    --build-arg CLI_BUILDER_IMAGE="$mcrCliBuilderImage" \
    -t $buildStackImage \
    --target build

echo "$baseImage" >> $ACR_BUILDER_IMAGES_ARTIFACTS_FILE
echo "$runStackImage" >> $ACR_BUILDER_IMAGES_ARTIFACTS_FILE
echo "$buildStackImage" >> $ACR_BUILDER_IMAGES_ARTIFACTS_FILE
echo "-------------------------------------------------"

# Copy buildpack/bin/template.build over to buildpack/bin/build and replace placeholders
buildFileTemplate="$SCRIPT_DIR/buildpack/bin/template.build"
targetBuildFile="$SCRIPT_DIR/buildpack/bin/build"
cp "$buildFileTemplate" "$targetBuildFile"
sed -i "s|$ORYX_AI_CONNECTION_STRING_PLACEHOLDER|$APPLICATION_INSIGHTS_CONNECTION_STRING|g" "$targetBuildFile"
sed -i "s|$ORYX_SDK_STORAGE_BASE_URL_PLACEHOLDER|$storageAccountUrl|g" "$targetBuildFile"

# Copy template.buildpack.toml over to buildpack.toml and replace placeholders
buildpackTomlTemplate="$SCRIPT_DIR/buildpack/template.buildpack.toml"
targetBuildpackToml="$SCRIPT_DIR/buildpack/buildpack.toml"
cp "$buildpackTomlTemplate" "$targetBuildpackToml"
sed -i "s|$ORYX_BUILDPACK_VERSION_PLACEHOLDER|$buildpackVersion|g" "$targetBuildpackToml"

# Packaging buildpack
buildPackImage="$destinationFqdn/$destinationRepo:buildpack-$builderImageVersion"
echo
echo "Packaging buildpack image: $buildPackImage"
echo
pack buildpack package $buildPackImage --config $SCRIPT_DIR/packaged-buildpack/package.toml
echo "$buildPackImage" >> $ACR_BUILDER_IMAGES_ARTIFACTS_FILE
echo "-------------------------------------------------"

# replace image tags with their MCR equivalent
mcrBaseImage=$(replaceRepo "$baseImage" "$MCR_BUILDER_IMAGE_REPO")
mcrRunStackImage=$(replaceRepo "$runStackImage" "$MCR_BUILDER_IMAGE_REPO")
mcrBuildStackImage=$(replaceRepo "$buildStackImage" "$MCR_BUILDER_IMAGE_REPO")
mcrBuildPackImage=$(replaceRepo "$buildPackImage" "$MCR_BUILDER_IMAGE_REPO")

# Copy template.builder.toml over to builder.toml and replace placeholders
builderTomlTemplate="$SCRIPT_DIR/builder/template.builder.toml"
targetBuilderToml="$SCRIPT_DIR/builder/builder.toml"
cp "$builderTomlTemplate" "$targetBuilderToml"
sed -i "s|$ORYX_BUILDPACK_IMAGE_PLACEHOLDER|$mcrBuildPackImage|g" "$targetBuilderToml"
sed -i "s|$ORYX_BUILDPACK_VERSION_PLACEHOLDER|$buildpackVersion|g" "$targetBuilderToml"
sed -i "s|$ORYX_RUN_STACK_IMAGE_PLACEHOLDER|$mcrRunStackImage|g" "$targetBuilderToml"
sed -i "s|$ORYX_BUILD_STACK_IMAGE_PLACEHOLDER|$mcrBuildStackImage|g" "$targetBuilderToml"

# Creating builder image
builderImage="$destinationFqdn/$destinationRepo:$builderImageVersion"
echo
echo "Creating builder image: $builderImage"
echo
pack builder create $builderImage --config $SCRIPT_DIR/builder/builder.toml
echo "$builderImage" >> $ACR_BUILDER_IMAGES_ARTIFACTS_FILE
echo "-------------------------------------------------"