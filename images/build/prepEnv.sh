#!/bin/bash

ORYX_BLOB_URL_BASE=https://oryxsdks.blob.core.windows.net/sdks/

downloadSdkAndExtract() {
    local platformName=$(echo $1 | sed 's/=.*$//')
    local version=$(echo $1 | sed 's/^.*=//')
    
    platformDir="/opt/$platformName"
    targetDir="$platformDir/$version"
    if [ -d "$targetDir" ]; then
      echo "$platformName version '$version' already exists. Skipping installing it..."
    else
      echo "Downloading and extracing version '$version' of platform '$platformName'..."
      cd /tmp
      curl -SL $ORYX_BLOB_URL_BASE/$platformName-$version.tar.gz --output $version.tar.gz
      mkdir -p "$targetDir"
      tar -xzf $version.tar.gz -C "$targetDir"

      # Create a link : major.minor => major.minor.patch
      cd "$platformDir"
      IFS='.' read -ra SDK_VERSION_PARTS <<< "$version"
      MAJOR_MINOR="${SDK_VERSION_PARTS[0]}.${SDK_VERSION_PARTS[1]}"
      echo
      echo "Creating link from $MAJOR_MINOR to $version..."
      ln -s $version $MAJOR_MINOR
    fi
}

while [[ $1 = *"="* ]]; do
  downloadSdkAndExtract "$1"
  shift
done