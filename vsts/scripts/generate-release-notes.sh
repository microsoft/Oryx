#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# This script reads `./CHANGELOG.md` file and produces a file that is added as a build artifact which
# contains the changes only to a partcular build.
# To achieve this, we use the tags that the release adds to the git repo, and do a `git diff` between the
# changelog file in that tag and HEAD. The output of this diff is later parsed to only output the new lines.
# In order for this script to work, the agent running it should have the full git repo available.

set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
CHANGELOG_FILE="$DIR/../CHANGELOG.md"

OUTPUT_FILE=$1
if [ -z "$OUTPUT_FILE" ]; then
    # Create a default file using DevOps' pipeline artifacts directory
    OUTPUT_FILE=$BUILD_ARTIFACTSTAGINGDIRECTORY/Release-notes.md
fi

echo "Release notes will be placed in $OUTPUT_FILE"

# First, we look for the latest tag that was pushed. Since our builds numbers are lexicographically ordered,
# YYYYMMDD.P, we just take the latest value that starts with a `2` to avoid other tags that might be in the repo.
# Optimistic note: yes, this script will break in year 3000, but we can fix it then.
LAST_TAG=$(git tag --list 2* | sort -r | head -n 1)

if [ -z "$LAST_TAG" ]; then
    echo "Couldn't find a base tag, will output the entire file"
    # Ignore the lines starting with [//] which we're using as comments.
    cat ../CHANGELOG.md | grep -v -e '^\[//\]' > $OUTPUT_FILE
else
    echo "Getting the diff from latest tag, $LAST_TAG"
    # Get the diff for the changelog file
    # The regex ^+[^+] is used to capture only the added lines, and the [^+], which means "exclude '+', removes the
    # lines that git adds to the diff output containing the file name. Finally, we remove the '+' from the beginning
    # of the selected lines.    
    git diff $LAST_TAG HEAD | grep -e ^+[^+] | sed 's/^+//' > $OUTPUT_FILE
fi
