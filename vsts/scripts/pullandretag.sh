#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail
# $1 > buildimage-acr.txt
# $2 > runtime-images-acr.txt
declare integrationtestfilter="oryxdevmcr.azurecr.io/public/oryx"

echo "Build image filter is set"
while read buildImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  if [[ $buildImage != *:latest ]]; then
	echo "Pulling the build image $buildImage ..."
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
done <"$1"

# Extract language string from string (e.g extract 'python' from 'category=python')
if [ -n "$TESTINTEGRATIONCASEFILTER" ];then
	# For DB tests we want all the runtime images to be present at thae agent machine
	if [[ "$TESTINTEGRATIONCASEFILTER" != *db* ]];then
		integrationtestfilter=$(echo $TESTINTEGRATIONCASEFILTER | cut -d'=' -f 2)
	fi
fi

# Always convert filter for runtime images to lower case
# integrationtestfilter=$(echo "$integrationtestfilter" | sed 's/.*/\L&/')
echo "Runtime image filter is set for "$integrationtestfilter

while read sourceImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  if [[ "$sourceImage" != *:latest ]]; then
	if [[ "$sourceImage" == *"$integrationtestfilter"* ]]; then
		echo "Pulling the runtime image $sourceImage ..."
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