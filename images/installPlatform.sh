#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------
set -e

# https://medium.com/@Drew_Stokes/bash-argument-parsing-54f3b81a6a8f
PARAMS=""
while (( "$#" )); do
  case "$1" in
    -u|--base-download-url)
      baseDownloadUrl=$2
      shift 2
      ;;
    -b|--base-target-dir)
      baseTagetDir=$2
      shift 2
      ;;
    --use-specified-dir)
      useSpecifiedDir="true"
      shift 1
      ;;
    -p|--platform)
      platformName=$2
      shift 2
      ;;
    -v|--platform-version)
      platformVersion=$2
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

if [ -z "$platformName" ]; then
    echo 'Platform name cannot be empty. Use the switch "-p" or "--platform" to specify the platform to install.'
    exit 1
fi

if [ -z "$platformVersion" ]; then
    echo 'Platform version cannot be empty. Use the switch "-v" or "--platform-version" to specify the version of platform to install.'
    exit 1
fi

if [ -z "$baseTagetDir" ]; then
  if [ -z "$DYNAMIC_INSTALL_ROOT_DIR" ]; then
    echo 'Base directory not provided. Either set the environment variable "DYNAMIC_INSTALL_ROOT_DIR" or use the switch "-b" or "--base-target-dir" to specify the base directory to install.'
    exit 1
  else
    baseTagetDir="$DYNAMIC_INSTALL_ROOT_DIR"
  fi
fi

if [ -z "$baseDownloadUrl" ]; then
  if [ -z "$SDK_STORAGE_BASE_URL" ]; then
    echo 'Base download url cannot be empty. Use the switch "-u" or "--base-download-url" to specify the base url from where the sdk can be installed.'
    exit 1
  else
    baseDownloadUrl="$SDK_STORAGE_BASE_URL"
  fi
fi

downloadedFileName="$platformVersion.tar.gz"

if [ "$useSpecifiedDir" == "true" ]; then
  dirToInstall="$baseTagetDir"
else
  dirToInstall="$baseTagetDir/$platformName/$platformVersion"
  rm -rf "$dirToInstall"
fi

PLATFORM_SETUP_START=$SECONDS
echo
echo "Downloading and extracting '$platformName' version '$platformVersion' to '$dirToInstall'..."
mkdir -p "$dirToInstall"
cd "$dirToInstall"
PLATFORM_BINARY_DOWNLOAD_START=$SECONDS
curl -D headers.txt \
    -SL \
    "$baseDownloadUrl/$platformName/$platformName-$platformVersion.tar.gz" \
    --output $downloadedFileName >/dev/null 2>&1
PLATFORM_BINARY_DOWNLOAD_ELAPSED_TIME=$(($SECONDS - $PLATFORM_BINARY_DOWNLOAD_START))
echo "Downloaded in $PLATFORM_BINARY_DOWNLOAD_ELAPSED_TIME sec(s)."
echo "Verifying checksum..."
headerName="x-ms-meta-checksum"
checksumHeader=$(cat headers.txt | grep -i $headerName: | tr -d '\r')
checksumHeader=$(echo $checksumHeader | tr '[A-Z]' '[a-z]')
checksumValue=${checksumHeader#"$headerName: "}
rm -f headers.txt
echo "$checksumValue $downloadedFileName" | sha512sum -c - >/dev/null 2>&1
echo "Extracting contents..."
tar -xzf "$downloadedFileName" -C .
rm -f "$downloadedFileName"
PLATFORM_SETUP_ELAPSED_TIME=$(($SECONDS - $PLATFORM_SETUP_START))
echo "Done in $PLATFORM_SETUP_ELAPSED_TIME sec(s)."
echo
echo > $dirToInstall/.oryx-sdkdownload-sentinel
