#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail
# $1 > file that has all the tags 

imagesFile="$1"

echo "base image tag file is in directory: $1"

if [ -f "$imagesFile" ]; then
    echo "$imagesFile exists. pushing buster image tags ..."
    while read imageName; do
      # read the tags from file
      echo "Pushing image $imageName ..."
      docker push "$imageName"
    done <"$imagesFile"
fi


