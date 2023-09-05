#!/bin/bash

declare -r __DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
source "$__DIR/../../../build/__phpVersions.sh"

# Please make sure that any changes to debian flavors supported here are also reflected in build/constants.yaml
declare -r VERSION_ARRAY_BOOKWORM=()
declare -r VERSION_ARRAY_BULLSEYE=($PHP74_VERSION $PHP80_VERSION $PHP81_VERSION $PHP82_VERSION)
declare -r VERSION_ARRAY_BUSTER=($PHP74_VERSION $PHP80_VERSION $PHP81_VERSION $PHP82_VERSION)
