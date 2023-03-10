#!/bin/bash

# branch name is of the format: refs/heads/sourceBranch
# sourceBranch name can be "username/branchName" or "branchName"
replacingText="refs/heads/"
sourceBranch=$(echo "$BUILD_SOURCEBRANCH" | sed -e "s.$replacingText..g")

if [[ "$sourceBranch" == */* ]]; then
    # swap '/' with '-' in sourceBranch name
    # ex: username/branchName -> username-branchName
    tagName="$sourceBranch"
    tagName=${tagName//\//-}
    # tag name should be like: username-branchName-20220310.1
    tagName="$tagName-$BUILD_BUILDNUMBER"
else
    # tag name should be like: branchName-20220310.1
    tagName="$sourceBranch-$BUILD_BUILDNUMBER"
fi

echo "Setting release tag name to '$tagName'..."
echo "##vso[task.setvariable variable=RELEASE_TAG_NAME;]$tagName"