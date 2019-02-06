#! /usr/bin/env bash

__run_dir=$(pwd)
__script_dir=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
dockerfile_path=Dockerfile.oryx

# creating a task for the registry requires checking in
# the Dockerfile.oryx file since it must be present in the
# clone
function task-for-registry () {
    local registry_name=${1}
    local registry_group_name=${2}
    local image_name=${3}
    local context_url=${4}
    local runtime=${5:-"node-10.14"}

    cp "${__script_dir}/${dockerfile_path}" "${__run_dir}/${dockerfile_path}"
    >&2 echo "you must checkin and push ${dockerfile_path}"

    task_name=image-builder
    az acr task create \
        --name ${task_name} \
        --registry ${registry_name} \
        --resource-group ${registry_group_name} \
        --context ${context_url} \
        --file ${dockerfile_path} \
        --commit-trigger-enabled true \
        --arg "RUNTIME=${runtime}"

    # az acr task run --name ${task_name} --registry ${registry_name}
}

task-for-registry $@