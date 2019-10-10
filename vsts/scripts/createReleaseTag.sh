#!/bin/bash

set -ex

# branch name is of the format: refs/heads/patch/21090924.1
replacingText="refs/heads/"
sourceBranch=$(echo "$BUILD_SOURCEBRANCH" | sed -e "s.$replacingText..g")

tagName=""
if [ "$sourceBranch" == "master" ]; then
    tagName="$BUILD_BUILDNUMBER"
elif [[ "$sourceBranch" == patch/* ]]; then
    IFS=/
    read -ra branchNameParts <<< "$sourceBranch"

    # Name of the tag which is being patched (ex: 20190730.1)
    patchedTagName=${branchNameParts[1]}

    # We want tags for patch releases in the format: 20190730.1-patch1, 20190730.1-patch2 etc.
    baseReleaseTagUrl="https://github.com/microsoft/Oryx/releases/tag"
    patchNumber=0

    # Increment patch numbers until we find one for which we have not created a release already
    while true; do
        patchNumber=$((patchNumber + 1))
        fullPatchTagName="$patchedTagName-patch$patchNumber"
        releaseUrl="$baseReleaseTagUrl/$fullPatchTagName"

        curl -I "$releaseUrl" 1> /tmp/createReleaseTag.txt 2> /dev/null
        grep "HTTP/1.1 404 Not Found" /tmp/createReleaseTag.txt &> /dev/null
        exitCode=$?
        rm -f /tmp/createReleaseTag.txt
        if [ $exitCode -eq 0 ]; then
            tagName="$fullPatchTagName"
            break
        fi
    done
fi

echo "##vso[task.setvariable variable=ReleaseTagName;]$tagName"