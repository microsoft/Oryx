#! /usr/bin/env bash

__run_dir=$(pwd)
__script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
dockerfile_path=Dockerfile.oryx

# builds current directory into image
function build-for-registry () {
    local registry_name=${1}
    local registry_group_name=${2}
    local image_name=${3:-"oryx-test:latest"}
    local context=${4:-"."}
    local runtime=${5:-"node-10.14"}

    cp "${__script_dir}/${dockerfile_path}" "${__run_dir}/${dockerfile_path}"

    az acr build \
        --registry ${registry_name} \
        --resource-group ${registry_group_name} \
        --file ${dockerfile_path} \
        --build-arg "RUNTIME=${runtime}" \
        --image ${image_name} \
        ${context}
    
    rm "${__run_dir}/${dockerfile_path}"
}

build-for-registry $@