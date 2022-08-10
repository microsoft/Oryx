#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# This script calculates container sizes after building a sample app with a given image.
#
# Input: docker image that should be used to base the containers off of
# Output: A file that contains the names of the sample apps used, and the size of the container
#   after the sample app was built.
set -e

declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

dockerImage=$1

function buildSampleAppAndCalculateSize() {
    platform=$1
    sampleApp=$2
    name="sample-app-$sampleApp"
    echo
    echo "Creating container for $platform/$sampleApp with name $name..."
    docker run --name $name -v "/$REPO_DIR/tests/SampleApps/$platform/$sampleApp://app" $dockerImage oryx build //app >/dev/null 2>&1
    docker ps --size -as -f "name=$name" --format "$platform/$sampleApp size: {{.Size}}" >> $resultFileName
    echo "Finished. Removing container $name..."
    docker rm $name >/dev/null 2>&1
}

resultFileName="/tmp/sample-app-container-sizes.txt"

echo "Container sizes being written to output file: $resultFileName"

echo "Sample app container sizes for $dockerImage" > $resultFileName
echo "-------------------------------------------" >> $resultFileName

buildSampleAppAndCalculateSize "DotNetCore" "NetCoreApp31.MvcApp"
buildSampleAppAndCalculateSize "golang" "hello-world"
buildSampleAppAndCalculateSize "hugo" "hugo-sample"
buildSampleAppAndCalculateSize "java" "MavenSimpleJavaApp"
buildSampleAppAndCalculateSize "nodejs" "helloworld-nuxtjs"
buildSampleAppAndCalculateSize "php" "greetings"
buildSampleAppAndCalculateSize "python" "flask-app"
buildSampleAppAndCalculateSize "ruby" "Jekyll-app"

echo
echo "Finished all sample apps. Printing contents of output file $resultFileName..."
echo
echo
cat $resultFileName

