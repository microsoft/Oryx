#!/bin/bash

declare -r __DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null && pwd )"
source "$__DIR/../../../build/__phpVersions.sh"
declare -r VERSION_ARRAY=($PHP73_VERSION $PHP72_VERSION)