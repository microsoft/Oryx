#!/bin/bash

declare -r BUILD_NUMBER="$BUILD_BUILDNUMBER"

echo
echo "Stop running containers:"
echo
docker ps -q | xargs --no-run-if-empty docker stop

echo
echo "Current list of docker images:"
echo
docker images

# An image that is built in our pipelines is tagged with 'latest' and '<build number>'.
# The following is to untag an image with the build number tag so that when the next time
# images are built, the older images can become dangled which can later be cleaned up.
#
# Note that we should untag these images only after they have been pushed to a remote repository.
imagePattern="oryxdevms/*:$BUILD_NUMBER"
imagesToUntag=$(docker images --filter=reference="$imagePattern" --format "{{.Repository}}:{{.Tag}}")
echo
echo "Found following images having the pattern '$imagePattern'. Untagging them ..."
echo $imagesToUntag
echo
docker rmi -f $imagesToUntag

echo
echo "Updated list of docker images:"
echo
docker images

echo
echo "Running 'docker system prune' ..."
echo
#docker system prune -f

echo
echo "Updated list of docker images:"
echo
docker images