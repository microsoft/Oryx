#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# This script to be copied and used from runtime images release definition; not to be run locally.

while read sourceImage; do
  echo "$sourceImage"
  docker pull "$sourceImage"
  newtag=$(echo "$sourceImage" | sed -r 's/oryxdevms/oryxprod/')
  echo "$newtag"
  docker tag "$sourceImage" "$newtag"``
  docker push "$newtag"``
done <"$(System.DefaultWorkingDirectory)/_Oryx-Runtime/drop/images/dockerhub-runtime-images.txt"
