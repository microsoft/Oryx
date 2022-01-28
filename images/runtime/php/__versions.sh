#!/bin/bash

declare -r __DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
source "$__DIR/../../../build/__phpVersions.sh"
declare -r VERSION_ARRAY=($PHP73_VERSION $PHP72_VERSION $PHP70_VERSION $PHP56_VERSION)
declare -r VERSION_ARRAY_BUSTER=($PHP74_VERSION $PHP80_VERSION $PHP81_VERSION)