#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail
# $1 > buster-buildimage-acr.txt or stretch-buildimage-acr.txt
# $2 > runtime-images-acr.txt
declare imagefilter="oryxdevmcr.azurecr.io/public/oryx"

echo "Build image filter is set"
while read buildImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  if [[ $buildImage != *:latest ]]; then
	echo "Pulling the build image $buildImage ..."
	docker pull "$buildImage" | sed 's/^/     /'

	# Trim the build number tag and append the '':latest' to end of it
	newtag="${buildImage%:*}:latest"

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
		imagefilter=$(echo $TESTINTEGRATIONCASEFILTER | cut -d'=' -f 2)
	fi
fi

# Always convert filter for runtime images to lower case
echo "Runtime image filter is set for "$imagefilter

while read sourceImage; do
  # Always use specific build number based tag and then use the same tag to create a version tag and push it
  if [[ "$sourceImage" != *:latest ]]; then
	if [[ "$sourceImage" == *"$imagefilter"* ]]; then
		echo "Pulling the runtime image $sourceImage ..."
		docker pull "$sourceImage" | sed 's/^/     /'

		# Trim the build number tag and append the version to end of it
		image="${sourceImage%:*}"
		echo
		echo "image $image"
		tagName="${sourceImage#$image:*}"
		echo "tagName $tagName"
		version="${tagName%%-*}"

		if [[ "$tagName" == *-fpm* ]]; then
			version="$version"-fpm
		fi

		echo "version $version"
		newtag="$image:$version"

		echo
		echo "Tagging the source image with tag $newtag ..."
		docker tag "$sourceImage" "$newtag" | sed 's/^/     /'
		echo
		echo -------------------------------------------------------------------------------
	fi
  fi
done <"$2"
