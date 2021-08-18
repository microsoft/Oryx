#!/bin/bash

# This script is referenced from official docker library: 
# https://github.com/docker-library/golang/blob/master/Dockerfile-debian.template
# some of golang's build scripts are written in golang
# we purge system golang later to make sure our final image uses what we just built

set -eux

LANG=C.UTF-8
# GOLANG_MAJOR_VERSION=${GOLANG_VERSION:0:3}
INSTALLATION_PREFIX=/opt/go/$GOLANG_VERSION
debianFlavor=$DEBIAN_FLAVOR
golangSdkFileName=""

if [ "$debianFlavor" == "stretch" ]; then
	# Use default golang sdk file name
	echo "debianFlavor is stretch"
else
	golangSdkFileName=golang-$debianFlavor-$GOLANG_VERSION.tar.gz
	apt-get update; \
	apt-get install -y --no-install-recommends \
		autoconf \
		libssl-dev \
		zlib1g-dev \
		libreadline-dev \
		build-essential
	echo "debianFlavor is NOT stretch"
fi

# Try getting the keys 5 times at most
# /tmp/receiveGpgKeys.sh $GPG_KEY


ls /opt/
ls /opt/go/


compressedSdkDir="/tmp/compressedSdk"
mkdir -p $compressedSdkDir
cd "$INSTALLATION_PREFIX"
tar -zcf $compressedSdkDir/$golangSdkFileName .