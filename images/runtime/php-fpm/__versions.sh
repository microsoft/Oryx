#!/bin/bash

declare -r __DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
source "$__DIR/../../../build/__phpVersions.sh"
declare -r VERSION_ARRAY_BUSTER=($PHP80_VERSION)
declare -r VERSION_ARRAY_BULLSEYE=($PHP74_VERSION $PHP81_VERSION)
