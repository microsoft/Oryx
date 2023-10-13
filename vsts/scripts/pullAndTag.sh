#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail
# $1 > buildimage-acr.txt
declare imagefilter="oryxdevmcr.azurecr.io/public/oryx"

function tagBuildImageForIntegrationTest() {
	local devbuildImageName="$1"
	local devbuildImageType="$2"
	local buildImageFilter="$3"
	local buildImageTagFilter="$4"
	local buildDefName="$BUILD_DEFINITIONNAME"
	local buildNumber="$RELEASE_TAG_NAME"

	# Check if a build image filter was provided, and return early if it's not the suffix of the provided build image
	if [ -n "$buildImageFilter" ] && [[ "$devbuildImageName" != *"$buildImageFilter" ]];then
		return
	fi

	# Check if a build tag filter was provided, and return early if it doesn't match the provided build tag
	if [ -n "$buildImageTagFilter" ] && [ "$devbuildImageType" != "$buildImageTagFilter" ];then
		return
	fi

	# Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
	if [ -z "$devbuildImageType" ]; then
		buildImage=$devbuildImageName:$buildDefName.$buildNumber
		# Trim the build number tag and append the '':latest' to end of it
		newtag=$devbuildImageName:latest
	else
		buildImage=$devbuildImageName:$devbuildImageType-$buildDefName.$buildNumber
		newtag=$devbuildImageName:$devbuildImageType
	fi

	echo "Pulling the build image $buildImage ..."
 	docker pull "$buildImage" | sed 's/^/     /'

	echo
	echo "Tagging the source image with tag $newtag ..."

	docker tag "$buildImage" "$newtag" | sed 's/^/     /'
	echo
	echo -------------------------------------------------------------------------------

}

buildImageFilter=""
buildImageTagFilter=""
platformFilter=""
platformVersionFilter=""

if [ -n "$TESTINTEGRATIONCASEFILTER" ];then
	IFS='&'
	read -a splitArr <<< "$TESTINTEGRATIONCASEFILTER"
	for val in "${splitArr[@]}";
	do
		if [[ "$val" == "category="* ]];then
			categoryPrefix="category="
			strippedVal=${val#"$categoryPrefix"}
			IFS='-'
			read -a tempSplitArr <<< "$strippedVal"
			len=${#tempSplitArr[@]}
			platformFilter="${tempSplitArr[0]}"
			if [[ $len -gt 1 ]];then
				platformVersionFilter="${tempSplitArr[1]}"
			fi
		fi

		if [[ "$val" == "build-image="* ]];then
			buildImagePrefix="build-image="
			strippedVal=${val#"$buildImagePrefix"}
			buildImageFilter="build"
			buildImageTagFilter="$strippedVal"
			if [[ "$strippedVal" == "cli-debian-stretch" ]];then
				buildImageFilter="cli"
				buildImageTagFilter="debian-stretch"
			elif [[ "$strippedVal" == "cli-debian-buster" ]];then
				buildImageFilter="cli"
				buildImageTagFilter="debian-buster"
			elif [[ "$strippedVal" == "cli-debian-bullseye" ]];then
				buildImageFilter="cli"
				buildImageTagFilter="debian-bullseye"
			elif [[ "$strippedVal" == "cli-builder-debian-bullseye" ]];then
				buildImageFilter="cli"
				buildImageTagFilter="builder-debian-bullseye"
			fi
		fi
	done
fi

echo "Build image filter is set for '$buildImageFilter:$buildImageTagFilter'"

tagBuildImageForIntegrationTest "$imagefilter/build" "debian-stretch" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "lts-versions-debian-stretch" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "lts-versions-debian-buster" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "azfunc-jamstack-debian-stretch" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "azfunc-jamstack-debian-buster" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "azfunc-jamstack-debian-bullseye" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "github-actions-debian-stretch" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "github-actions-debian-buster" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "github-actions-debian-bullseye" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "github-actions-debian-bookworm" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "vso-ubuntu-focal" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "vso-debian-bullseye" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "full-debian-buster" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/build" "full-debian-bullseye" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/cli" "debian-stretch" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/cli" "debian-buster" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/cli" "debian-bullseye" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/cli" "builder-debian-bullseye" "$buildImageFilter" "$buildImageTagFilter"
tagBuildImageForIntegrationTest "$imagefilter/pack" "" "$buildImageFilter" "$buildImageTagFilter"


# Extract language string from string (e.g extract 'python' from 'category=python', 'debian-stretch' from 'build-image=debian-stretch')
if [ -n "$TESTINTEGRATIONCASEFILTER" ];then
	# For DB tests we want only the build images to be present at the agent machine
	if [[ "$platformFilter" != "db" ]];then

		# Always convert filter for runtime images to lower case
		echo "Runtime image filter is set for $platformFilter with version $platformVersionFilter"

		# Create a local file that consolidates the different runtime image files into one to be read from
		# Note: we don't write this file to the drop folder as we don't want this file written to for every integration test job
		sourceFile="$BUILD_SOURCESDIRECTORY/temp/images/runtime-images-acr.txt"

		# Consolidate the different Debian runtime image files into one to be read from
		for FILE in $(find $BUILD_ARTIFACTSTAGINGDIRECTORY/drop/images -name 'runtime-images-acr.*.txt')
		do
  			(cat $FILE; echo) >> '$sourceFile'
		done

		while read sourceImage; do
  		# Always use specific build number based tag and then use the same tag
		# to create a version tag and push it
  			if [[ "$sourceImage" != *:latest ]]; then
				if [[ "$sourceImage" == *"$platformFilter:$platformVersionFilter"* ]]; then
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
		done <"$sourceFile"
	fi
fi

