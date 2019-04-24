#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

ACR_NAME='oryxdevmcr'
AZ_NAME_OUTPUT_PARAMS="--name $ACR_NAME --output tsv"

# Prepare an array with all repository names in the registry
REPOS=(`az acr repository list $AZ_NAME_OUTPUT_PARAMS`)
echo "Found ${#REPOS[@]} repositories in ACR instance '$ACR_NAME'"
echo

datecmd='date'
if [[ "$OSTYPE" == "darwin"* ]]; then datecmd='gdate'; fi
TIMESTAMP_CUTOFF=`$datecmd --iso-8601=seconds -d 'month ago'`

for repo in "${REPOS[@]}"
do
	azQuery="[?timestamp<=\`$TIMESTAMP_CUTOFF\`].digest"
	digests=(`az acr repository show-manifests $AZ_NAME_OUTPUT_PARAMS --repository $repo --orderby time_asc --query "$azQuery"`)

	echo "Deleting ${#digests[@]} images created before '$TIMESTAMP_CUTOFF' in repository '$repo'..."
	for manifest in "${MANIFESTS[@]}"
	do
		az acr repository delete --name $ACR_NAME --yes --image $manifest
		echo "> Deleted $manifest"
	done
	echo
done
