#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Cache common Node packages for faster build times.
# We currently only cache packages using Yarn, since it has a better caching story
# than npm.
set -e

YARN_CACHE_DIR="${1:-/usr/local/share/yarn-cache}"
echo "Caching packages for Yarn. Cache location is $YARN_CACHE_DIR"
mkdir -p  $YARN_CACHE_DIR

yarn config set cache-folder "$YARN_CACHE_DIR"

# Since there's no simple command for Yarn to add packages to the cache, we have to add them
# to an actual app. We create a temp folder to add the packages, which then can be deleted.
TEMP_APP_DIR=/tmp/node-package-cache-app
rm -fr $TEMP_APP_DIR || true
mkdir -p $TEMP_APP_DIR
cd $TEMP_APP_DIR

function installPackage() {
    pkg=$1
    yarn add $pkg > /dev/null 2> /dev/null
    echo "Package $pkg installed. New cache size: $(du -sh $YARN_CACHE_DIR | cut -f -1)"
}

# We install the most used packages according to our telemetry.
# Query to fetch it:
# let prefix = "Dependency: ";
# let packageRE = "([@/a-zA-Z-_]+),?([~^><=0-9]{0,2}[0-9.]*[-]?.*)";
# traces
# | where timestamp > ago(30d)
# | where message startswith prefix and customDimensions["platform"] == "nodejs"
# | project Package = substring(message, strlen(prefix)), 
#           customDimensions,
#           PackageName = extract(packageRE, 1, substring(message, strlen(prefix))),
#           PackageVersion = extract(packageRE, 2, substring(message, strlen(prefix))),
#           Website = operation_Name 
# | summarize by PackageName, Website // First summarize with Packages to count only the number of websites that use each package.
# | summarize Uses = count() by PackageName
# | order by Uses
# Then, we cut the list until the total cache reaches the target size
for pkg in \
    express \
    body-parser \
    dotenv \
    axios \
    morgan \
    cors \
    cookie-parser \
    moment \
    debug \
; do
    installPackage $pkg
done;

# We cache both latest versions and some specific version of most used packages to broaden the coverage.
for pkg in \
    express@^4.17.1 \
    express@^4.16.1 \
    body-parser@^1.19.0 \
    dotenv@^8.2.0 \
    axios@0.21.1 \
    axios@0.19.2 \
    morgan@^1.10.0 \
    morgan@^1.9.1 \
    cors@^2.8.5 \
    cookie-parser@^1.4.5 \
    cookie-parser@^1.4.4 \
    moment@^2.29.1 \
    moment@^2.24.0 \
    debug@~2.6.9 \
; do
    installPackage $pkg
done;

# Open up the cache so other users can consume it.
chmod -R 777 $YARN_CACHE_DIR

echo "Caching done. Total cache size: $(du -sh $YARN_CACHE_DIR | cut -f -1)"
rm -fr $TEMP_APP_DIR