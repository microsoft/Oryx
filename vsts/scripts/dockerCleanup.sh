#!/bin/bash

declare -r BUILD_NUMBER="$BUILD_BUILDNUMBER"
declare -r DOCKER_SYSTEM_PRUNE="${ORYX_DOCKER_SYSTEM_PRUNE:-true}"

echo
echo "Stop running containers:"
echo
docker ps -q | xargs --no-run-if-empty docker stop

function UntagImages(){
	local imagePattern=$1
	local imagesToUntag=$(docker images --filter=reference="$imagePattern" --format "{{.Repository}}:{{.Tag}}")

	echo
	echo "Found following images having the pattern '$imagePattern'. Untagging them ..."
	echo $imagesToUntag
	echo

	if [ ! -z "$imagesToUntag" ]
	then
		docker rmi -f $imagesToUntag
	fi
}

echo
echo "Docker size:"
echo
docker system df

echo
echo "Current list of docker images:"
echo
docker images

# An image that is built in our pipelines is tagged with 'latest' and '<build number>'.
# The following is to untag an image with the build number tag so that when the next time
# images are built, the older images can become dangled which can later be cleaned up.
#
# Note that we should untag these images only after they have been pushed to a remote repository.
UntagImages "oryxdevms/*:*.*"
UntagImages "oryxprod/*:latest"
UntagImages "oryxprod/*:*.*"
UntagImages "oryxprod/*:Release*"

echo
echo "Updated list of docker images:"
echo
docker images

echo
echo "Cleanup: Run 'docker system prune': $DOCKER_SYSTEM_PRUNE"
if [ $DOCKER_SYSTEM_PRUNE = "true" ]
then
	docker system prune -f

	echo
	echo "Updated list of docker images:"
	echo
	docker images
fi

echo
echo "Docker size:"
echo
docker system df