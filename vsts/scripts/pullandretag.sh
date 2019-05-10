#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euxo pipefail

while read sourceImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  if [[ $sourceImage != *:latest ]]; then
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
done <"$1"
