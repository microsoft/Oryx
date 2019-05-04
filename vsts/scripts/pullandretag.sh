#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

declare integrationtestfilter="oryxdevmcr.azurecr.io/public/oryx/"

echo "first arg: "$1
echo "second arg: "$2

echo "Build image filter is set"
while read buildImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  echo "image: "$buildImage
  if [[ $buildImage != *:latest ]]; then
	if [[ $buildImage == *"build"* ]]; then
		echo "Pulling the source image $buildImage ..."
		docker pull "$buildImage" | sed 's/^/     /'
        
		# Trim the build number tag and append the '':latest' to end of it
		newtag="${buildImage%:*}:latest"

		# Replace the ACR registry repository name with a name that the tests know about
		newtag=$(echo "$newtag" | sed 's,oryxdevmcr.azurecr.io/public/oryx,oryxdevms,g')

		echo
		echo "Tagging the source image with tag $newtag ..."
		docker tag "$buildImage" "$newtag" | sed 's/^/     /'
		echo
		echo -------------------------------------------------------------------------------
	fi
  fi
done <"$1"

if [ -n "$TESTINTEGRATIONCASEFILTER" ];then
	integrationtestfilter=$TESTINTEGRATIONCASEFILTER
fi

integrationtestfilter=$(echo "$integrationtestfilter" | sed 's/.*/\L&/')
echo "Runtime image filter is set for "$integrationtestfilter

while read sourceImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  echo "image: "$sourceImage
  if [[ $sourceImage != *:latest ]]; then
	if [[ $sourceImage == *"$integrationtestfilter"* ]]; then
		echo "Pulling the source image $sourceImage ..."
		docker pull "$sourceImage" | sed 's/^/     /'
        
		# Trim the build number tag and append the '':latest' to end of it
		newtag="${sourceImage%:*}:latest"

		# Replace the ACR registry repository name with a name that the tests know about
		newtag=$(echo "$newtag" | sed 's,oryxdevmcr.azurecr.io/public/oryx,oryxdevms,g')

		echo
		echo "Tagging the source image with tag $newtag ..."
		docker tag "$sourceImage" "$newtag" | sed 's/^/     /'
		echo
		echo -------------------------------------------------------------------------------
	fi
  fi
done <"$2"