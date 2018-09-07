#!/bin/bash

echo
echo "List of all containers"
docker ps -a

echo
echo "Stopping and removing all containers starting with prefix 'oryxtests_'..."

allContainers=$(docker ps -a --filter "name=oryxtests_" -q)

if [ -z "$allContainers" ]
then
    echo
    echo "Could not find any containers that were created by tests."
    exit 0
else
    docker rm -f -v $allContainers
fi

echo
echo "Updated list of all containers"
docker ps -a

