#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

# Cache common Node packages for faster build times.
# We currently only cache packages using Yarn, since it has a better caching story
# than npm.
set -e

YARN_CACHE_DIR=/usr/local/share/yarn-cache
echo "Caching packages for Yarn. Cache location is $YARN_CACHE_DIR"
mkdir -p  $YARN_CACHE_DIR

source /usr/local/bin/benv

yarn config set cache-folder "$YARN_CACHE_DIR"

# Since there's no simple command for Yarn to add packages to the cache, we have to add them
# to an actual app. We create a temp folder to add the packages, which then can be deleted.
TEMP_APP_DIR=/tmp/node-package-cache-app
rm -fr $TEMP_APP_DIR || true
mkdir -p $TEMP_APP_DIR
cd $TEMP_APP_DIR

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
# Then, we cut the list until the total cache size reaches 50MB
for pkg in \
    express \
    cookie-parser \
    body-parser \
    debug \
    moment \
    uuid \
    morgan \
    react \
    mongodb \
    http-errors \
    express-session \
    react-dom \
    jsonwebtoken \
    redis \
    ejs \
    request \
    axios \
    express-validator \
    jade \
    http-status \
    ioredis \
    cors \
    lodash \
    connect-redis \
    dotenv \
    moment-timezone \
    helmet \
    pug \
; do
    yarn add $pkg > /dev/null 2> /dev/null
    echo "Package $pkg installed. Folder size: $(du -sh $YARN_CACHE_DIR | cut -f -1)"
done;

echo "Caching done. Total cache size: $(du -sh $YARN_CACHE_DIR | cut -f -1)"
rm -fr $TEMP_APP_DIR