#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -e

declare -r createBugFileContent='[{"op": "add","path": "/fields/System.Title","from": null,"value": "Build Failed-'$BUILD_DEFINITIONNAME'-'$BUILD_BUILDNUMBER'"}, \
                                                                  {"op": "add", path": "/fields/System.AreaPath", "value": "DevDiv\Azure Developer Experience\Oryx"}, \
                                                                  {"op": "add", "path": "/fields/Microsoft.DevDiv.OrgRank","value": "9999"}]'
if [ "$BUILD_SOURCEBRANCH" == "refs/heads/master" ] && [ "$BUILD_REASON" == "Schedule" ] && \
   ( [ "$BUILD_DEFINITIONNAME" == "Oryx-Nightly" ] || [ "$BUILD_DEFINITIONNAME" == "Oryx-CI" ] ); then
        echo "here i am"
        token=$DEVDIV_TOKEN
        encryptedToken="$(echo -n ':$token' | iconv -f UTF-8 -t UTF-16LE | base64)"
        echo $encryptedToken
        echo -e $createBugFileContent>postbug.data
        requestURI="https://dev.azure.com/devdiv/0bdbc590-a062-4c3f-b0f6-9383f67865ee/_apis/wit/workItems/\$Bug?api-version=5.0"
        curl -v -H "Authorization: Basic $encryptedToken"  -H "Content-Type:application/json-patch+json" -d @postbug.data -L $requestURI
fi