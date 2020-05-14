#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

declare -r REPO_DIR_IMAGE=$( cd $( dirname "$0" ) && cd .. && pwd )

runtimeSubDir="$1"
runtimeImageBaseType="$2"

if [ "$runtimeImageBaseType" == "buster" ]
then
    source $REPO_DIR_IMAGE/build/buildRunTimeImageBasesBuster.sh $runtimeSubDir
else
    source $REPO_DIR_IMAGE/build/buildRunTimeImageBasesStretch.sh $runtimeSubDir
fi
