#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

ACR_NAME='oryxdevmcr'

# Prepare an array with all repository names in the registry
REPOS=(`az acr repository list --name $ACR_NAME --output tsv`)
echo "Found ${#REPOS[@]} repositories in ACR instance '$ACR_NAME'"
echo

datecmd='date'
if [[ "$OSTYPE" == "darwin"* ]]; then datecmd='gdate'; fi
TIMESTAMP_CUTOFF=`$datecmd --iso-8601=seconds -d 'month ago'`

for repo in "${REPOS[@]}"
do
	echo "Deleting images created before '$TIMESTAMP_CUTOFF' in repository '$repo'..."
	az acr repository show-manifests --repository $repo --name $ACR_NAME --orderby time_asc -o json --query "[? timestamp >=\`$TIMESTAMP_CUTOFF\`]"
	exit 0
done