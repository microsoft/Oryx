#!/bin/bash
# This script that builds the builder image using the dockerfile in this directory,
# and pushes it to a specified azure container registry. Users must specify an 
# ACR name to push the image to, and have the option to override the image repo and tag for the image.
SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
printf -v DATE_TAG '%(%Y-%m-%d-%H-%M-%S)T' -1

# default values for non-required parameters
builderRepo="public/oryx/builder"
builderTag="capps-$DATE_TAG"
baseBuilderTag="20230208.1"

PARAMS=""
while (( "$#" )); do
  case "$1" in
    -n|--acr-name)
      acrName=$2
      shift 2
      ;;
    -r|--builder-repo)
      builderRepo=$2
      shift 2
      ;;
    -t|--builder-tag)
      builderTag=$2
      shift 2
      ;;
    -b|--base-builder-tag)
      baseBuilderTag=$2
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

if [[ -z $acrName ]]; then
  echo "Error: Must supply a value for the required '-n|--acr-name' parameter."
  exit 1
fi

BUILD_IMAGE="$acrName.azurecr.io/$builderRepo:$builderTag"

echo "Building '$BUILD_IMAGE'..."
echo
cd $SCRIPT_DIR
docker build \
  --build-arg BASE_BUILDER_TAG=$baseBuilderTag \
  -t $BUILD_IMAGE \
  -f Dockerfile \
  .
echo
echo "Pushing '$BUILD_IMAGE'..."
docker push $BUILD_IMAGE