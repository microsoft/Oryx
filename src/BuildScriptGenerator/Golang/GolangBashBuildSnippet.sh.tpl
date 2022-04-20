#!/bin/bash
set -e

# This script will build golang applications
# Requirements:
#	./go.mod
#	./main.go

GOLANG_BUILD_START_TIME=$SECONDS
echo "   "
echo "Using Golang version: "
go version
echo "   " 
echo "   "

# TODO: look into go tidy
#		which removed unused dependencies
#		look into go vendor, which caches dependencies

# TODO: add support for nested dirs
echo "building go app..."
go build -o oryxBuildBinary

echo "list of module dependencies"
go list -m
GOLANG_ELAPSED_TIME=$(($SECONDS - $GOLANG_BUILD_START_TIME))
echo "Golang app built in $GOLANG_ELAPSED_TIME sec(s)."