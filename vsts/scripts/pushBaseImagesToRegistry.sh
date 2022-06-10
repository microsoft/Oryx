#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail
# $1 > file that has all the tags 
# e.g. /data/agent/_work/206/a/drop/images/yarn-cache-build/oryxmcr-base-images-mcr.txt

bullseyeBaseImagesFile="$1"-bullseye-base-images-mcr.txt
busterBaseImagesFile="$1"-buster-base-images-mcr.txt
stretchBaseImagesFile="$1"-stretch-base-images-mcr.txt
bullseyeBaseImagesFile="$1"-bullseye-base-images-mcr.txt

echo "base image tag file is in directory: $1"

if [ -f "$bullseyeBaseImagesFile" ]; then
    echo "$bullseyeBaseImagesFile exists. pushing bullseye image tags ..."
    while read imageName; do
      # read the tags from file
      echo "Pushing image $imageName ..."
      docker push "$imageName"
    done <"$bullseyeBaseImagesFile"
fi

if [ -f "$busterBaseImagesFile" ]; then
    echo "$busterBaseImagesFile exists. pushing buster image tags ..."
    while read imageName; do
      # read the tags from file
      echo "Pushing image $imageName ..."
      docker push "$imageName"
    done <"$busterBaseImagesFile"
fi

if [ -f "$stretchBaseImagesFile" ]; then
    echo "$stretchBaseImagesFile exists. pushing stretch image tags ..."
    while read imageName; do
      # read the tags from file
      echo "Pushing image $imageName ..."
      docker push "$imageName"
    done <"$stretchBaseImagesFile"
fi

if [ -f "$bullseyeBaseImagesFile" ]; then
    echo "$bullseyeBaseImagesFile exists. pushing bullseye image tags ..."
    while read imageName; do
      # read the tags from file
      echo "Pushing image $imageName ..."
      docker push "$imageName"
    done <"$bullseyeBaseImagesFile"
fi
