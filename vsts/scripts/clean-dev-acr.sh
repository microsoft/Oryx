#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r ACR_NAME='oryxdevmcr'
declare -r AZ_NAME_OUTPUT_PARAMS="--name $ACR_NAME --output tsv"

if [[ "$1" == "--yes" ]]; then
	YES="$1"
fi

# Prepare an array with all repository names in the registry
REPOS=(`az acr repository list $AZ_NAME_OUTPUT_PARAMS`)
echo "Found ${#REPOS[@]} repositories in ACR instance '$ACR_NAME'"
echo

datecmd='date'
if [[ "$OSTYPE" == "darwin"* ]]; then datecmd='gdate'; fi

tsLimit=`$datecmd --iso-8601=seconds -d '2 months ago'`
declare -r azQuery="[?timestamp<=\`$tsLimit\`].digest"

for repo in "${REPOS[@]}"
do
	digests=(`az acr repository show-manifests $AZ_NAME_OUTPUT_PARAMS --repository $repo --orderby time_asc --query "$azQuery"`)

	echo "Deleting ${#digests[@]} images created before '$tsLimit' in repository '$repo'..."
	for manifest in "${MANIFESTS[@]}"
	do
		az acr repository delete --name $ACR_NAME $YES --image $manifest
		echo "> Deleted $manifest"
	done
	echo
done
