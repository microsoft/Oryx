#!/bin/bash
set -e

declare -r BUILD_NUMBER="$BUILD_BUILDNUMBER"
declare -r DOCKER_SYSTEM_PRUNE="${ORYX_DOCKER_SYSTEM_PRUNE:-true}"

# The following code snippet is a hack to workaround the following issue:
# Our CI agents run under a non-root account, but docker containers run under the 'root' account.
# Since a non-root account cannot delete files created by a 'root' account, we try to run a
# docker container and run the clean up command from there using the volume mounted directory trick.
# 
# This is required for end-to-end tests to work, where a host's directory is volume mounted to a build
# image and the same host directory is volume mounted to a runtime image too.
#
# Even though Docker has a way to run a container as a non-root (for example, in our case the CI agent account),
# this causes problems in build image where we try to install packages and stuff and we run into permission problems.
# Since that is too intrusive and error prone, this hack allows us to not worry about those permission issues.
mountedDirs="/tmp/OryxTestsMountedDirs"
if [ -d "$mountedDirs" ]; then
	echo
	echo "Cleaning up files created by tests' docker containers ..."
	docker run -v $mountedDirs:/tempDirs oryxdevms/build /bin/bash -c "rm -rf /tempDirs/* && ls /tempDirs"
fi

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
