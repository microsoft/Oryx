#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -euo pipefail
# $1 > file that has all the tags 
# e.g. /data/agent/_work/206/a/drop/images/yarn-cache-build/oryxmcr-base-images-mcr.txt

cat "$1"

while read imageName; do
  # read the tags from file
  echo "Pushing image $imageName ..."
  docker push "$imageName"
done <"$1"
