#!/bin/bash
set -e

# This script will build golang applications
# Requirements:
#	./go.mod
#	./main.go

# enables golang globally
export PATH="/tmp/oryx/platforms/golang/{{ GolangVersion }}/go/bin/:$PATH"
echo "PATH: $PATH"

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
doc="https://aka.ms/troubleshoot-go"
suggestion="Please check your go.mod is valid.
 Try building locally first with the following command: go build"
msg="${suggestion} | ${doc}"
cmd="go build -o oryxBuildBinary"
LogErrorWithTryCatch "$cmd" "$msg"

echo "list of module dependencies"
go list -m
GOLANG_ELAPSED_TIME=$(($SECONDS - $GOLANG_BUILD_START_TIME))
echo "Golang app built in $GOLANG_ELAPSED_TIME sec(s)."