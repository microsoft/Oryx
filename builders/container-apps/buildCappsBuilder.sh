#!/bin/bash
set -exo pipefail

# This script that builds the builder image using the dockerfile in this directory,
# and pushes it to a specified azure container registry. Users must specify an 
# ACR name to push the image to, and have the option to override the image repo and tag for the image.
declare -r SCRIPT_DIR=$( cd $( dirname "$0" ) && pwd )
declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && cd .. && pwd )
source $REPO_DIR/build/__variables.sh

# default values for non-required parameters
destinationFqdn="oryxprodmcr.azurecr.io"
destinationRepo="public/oryx/builder"
destinationTag="capps-20230208.1"

PARAMS=""
while (( "$#" )); do
  case "$1" in
    -f|--destination-fqdn)
      destinationFqdn=$2
      shift 2
      ;;
    -r|--destination-repo)
      destinationRepo=$2
      shift 2
      ;;
    -t|--destination-tag)
      destinationTag=$2
      shift 2
      ;;
    -b|--base-builder-tag)
      baseBuilderImage=$2
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

echo "Initializing artifacts file: $ACR_CAPPS_BUILDER_IMAGES_ARTIFACTS_FILE"
mkdir -p "$ARTIFACTS_DIR/images"
touch $ACR_CAPPS_BUILDER_IMAGES_ARTIFACTS_FILE
> $ACR_CAPPS_BUILDER_IMAGES_ARTIFACTS_FILE

BUILD_IMAGE="$destinationFqdn/$destinationRepo:$destinationTag"
echo "Building '$BUILD_IMAGE'..."
echo
cd $SCRIPT_DIR
if [[ -z "$baseBuilderImage" ]]
then
  docker build \
    -t $BUILD_IMAGE \
    -f Dockerfile \
    .
else
  docker build \
    -t $BUILD_IMAGE \
    --build-arg BASE_BUILDER_IMAGE=$baseBuilderImage \
    -f Dockerfile \
    .
fi
echo
echo "$BUILD_IMAGE" >> $ACR_CAPPS_BUILDER_IMAGES_ARTIFACTS_FILE