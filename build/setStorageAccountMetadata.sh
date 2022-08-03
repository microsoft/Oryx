#!/bin/bash

storageAccount="https://oryxsdksdev.blob.core.windows.net"
container="dotnet"

xml=$(curl -k --silent "$storageAccount/$container?restype=container&comp=list")

nonStretchRegex="$container-(focal-scm|bullseye|buster)-(.*?).tar.gz"
stretchRegex="$container-(.*?).tar.gz"
grep -oPm1 "(?<=<Name>)[^<]+" <<< "$xml" | while read -r fileName ; do
    ostype=$(echo $fileName | awk "{match(\$0,/$nonStretchRegex/,a);print a[1]}" )
    version=$(echo $fileName | awk "{match(\$0,/$nonStretchRegex/,a);print a[2]}")
    if [[ -z $ostype ]] || [[ -z $version ]] ; then
        version=$(echo $fileName | awk "{match(\$0,/$stretchRegex/,a);print a[1]}")
        if [[ ! -z $version ]] ; then
            ostype="stretch"
        fi
    fi
    if [[ -z $ostype ]] || [[ -z $version ]] ; then
        echo "No match found for $fileName. Skipping..."
        continue
    fi
    metadata=$(curl -I --silent "$storageAccount/$container/$fileName?comp=metadata" | grep x-ms-meta | tr '\n' ' ' | sed 's/:\s/=/g' | sed 's/x-ms-meta-//g')
    if [ $ostype = "stretch" ] ; then
        metadataToAdd="Os_type=stretch"
        echo "Adding metadata to $fileName: $metadataToAdd"
        # az storage blob metadata update \
        # --container-name $container \
        # --name $fileName \
        # --account-name oryxsdksdev \
        # --sas-token $DEV_STORAGE_ACCOUNT_SAS_TOKEN 
        # --metadata $metadata Os_type=stretch
    else
        metadataToAdd="Os_type=$ostype Sdk_version=$version"
        echo "Adding metadata to $fileName: $metadataToAdd"
        # az storage blob metadata update \
        # --container-name $container \
        # --name $fileName \
        # --account-name oryxsdksdev \
        # --sas-token $DEV_STORAGE_ACCOUNT_SAS_TOKEN 
        # --metadata $metadata Os_type=$ostype Sdk_version=$version
    fi
done