#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -o pipefail

while read sourceImage; do
  # Always use specific build number based tag and then use the same tag to create a 'latest' tag and push it
  if [[ $sourceImage != *:latest ]]; then
    echo "Pulling the source image $sourceImage ..."
    docker pull "$sourceImage" | sed 's/^/     /'
        
    # Trim the build number and append the '':latest' to end of it
    newtag="${sourceImage%:*}:latest"
    echo
    echo "Tagging the source image with tag $newtag ..."
    docker tag "$sourceImage" "$newtag" | sed 's/^/     /'
    echo
    echo -------------------------------------------------------------------------------
  fi
done <"$1"