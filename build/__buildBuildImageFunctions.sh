#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

function BuildAndTagStage()
{
	local dockerFile="$1"
	local stageName="$2"
	local stageTagName="$ACR_PUBLIC_PREFIX/$2"

	echo
	echo "Building stage '$stageName' with tag '$stageTagName'..."
	docker build \
		--target $stageName \
		-t $stageTagName \
		$buildMetadataArgs \
		$BASE_TAG_BUILD_ARGS \
		-f "$dockerFile" \
		.
}

function buildDockerImage() {
	local dockerFileToBuild="$1"
	local dockerImageRepoName="$2"
	local dockerFileForTestsToBuild="$3"
	local dockerImageForTestsRepoName="$4"
	local dockerImageForDevelopmentRepoName="$5"
	local dockerImageBaseTag="$6"

	# Tag stages to avoid creating dangling images.
	# NOTE:
	# These images are not written to artifacts file because they are not expected
	# to be pushed. This is just a workaround to prevent having dangling images so that
	# when a cleanup operation is being done on a build agent, a valuable dangling image
	# is not removed.
	BuildAndTagStage "$dockerFileToBuild" node-install
	BuildAndTagStage "$dockerFileToBuild" dotnet-install
	BuildAndTagStage "$dockerFileToBuild" python

	# If no tag was provided, use a default tag of "latest"
	if [ -z "$dockerImageBaseTag" ]
	then
		dockerImageBaseTag="latest"
	fi

	builtImageTag="$dockerImageRepoName:$dockerImageBaseTag"
	docker build -t $builtImageTag \
		--build-arg AGENTBUILD=$BUILD_SIGNED \
		$BASE_TAG_BUILD_ARGS \
		--build-arg AI_KEY=$APPLICATION_INSIGHTS_INSTRUMENTATION_KEY \
		$storageArgs \
		$buildMetadataArgs \
		-f "$dockerFileToBuild" \
		.

	echo
	echo Building a base image for tests...
	# Do not write this image tag to the artifacts file as we do not intend to push it
	testImageTag="$dockerImageForTestsRepoName:$dockerImageBaseTag"
	docker build -t $testImageTag -f "$dockerFileForTestsToBuild" .

	echo "$dockerImageRepoName:$dockerImageBaseTag" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE

	# Retag build image with build number tags
	if [ "$AGENT_BUILD" == "true" ]
	then
		uniqueTag="$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"
		if [ "$dockerImageBaseTag" != "latest" ]
		then
			uniqueTag="$dockerImageBaseTag-$uniqueTag"
		fi

		echo
		echo "Retagging image '$builtImageTag' with ACR related tags..."
		docker tag "$builtImageTag" "$dockerImageRepoName:$dockerImageBaseTag"
		docker tag "$builtImageTag" "$dockerImageRepoName:$uniqueTag"

		# Write the list of images that were built to artifacts folder
		echo
		echo "Writing the list of build images built to artifacts folder..."

		# Write image list to artifacts file
		echo "$dockerImageRepoName:$uniqueTag" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
	else
		docker tag "$builtImageTag" "$dockerImageForDevelopmentRepoName:$dockerImageBaseTag"
	fi
}

function createImageNameWithReleaseTag() {
	local imageNameToBeTaggedUniquely="$1"
	# Retag build image with build number tags
	if [ "$AGENT_BUILD" == "true" ]
	then
		local uniqueImageName="$imageNameToBeTaggedUniquely-$BUILD_DEFINITIONNAME.$RELEASE_TAG_NAME"

		echo
		echo "Retagging image '$imageNameToBeTaggedUniquely' with ACR related tags..."
		docker tag "$imageNameToBeTaggedUniquely" "$uniqueImageName"

		# Write image list to artifacts file
		echo "$uniqueImageName" >> $ACR_BUILD_IMAGES_ARTIFACTS_FILE
	fi
}
