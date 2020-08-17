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
    installPackage $pkg
done;

# We cache both latest versions and some specific version of most used packages to broaden the coverage.
for pkg in \
    express@^4.16.4 \
    body-parser@^1.18.3 \
    cookie-parser@~1.4.3 \
    debug@~2.6.9 \
    express@~4.16.0 \
    http-errors@~1.6.2 \
    morgan@~1.9.0 \
    jade@~1.11.0 \
    cors@^2.8.5 \
    axios@^0.18.0 \
    express-session@^1.15.6 \
    request@^2.88.0 \
    morgan@^1.9.1 \
    dotenv@^6.2.0 \
    passport@^0.4.0 \
    ejs@^2.6.1 \
    moment@^2.24.0 \
    express@^4.16.3 \
    moment@^2.22.2 \
    prop-types@15.6.2 \
    cookie-parser@^1.4.3 \
    express@4.13.4 \
    lodash@^4.17.11 \
    body-parser@1.15.0 \
    axios@0.18.0 \
    react-dom@15.0.1 \
    react-fa@4.0.1 \
    classname@0.0.0 \
    uuid@2.0.2 \
    mongodb@2.1.14 \
    flux@2.1.1 \
    react@15.0.1 \
    mongodb-uri@0.9.7 \
    node-uuid@1.4.8 \
    pug@2.0.0-rc.1 \
    uuid@^3.3.2 \
    jquery@^3.3.1 \
    jsonwebtoken@^8.4.0 \
    bcryptjs@^2.4.3 \
    react-router-dom@^4.3.1 \
    redis@^2.8.0 \
    pug@2.0.0-beta11 \
    core-js@^2.5.4 \
    multer@^1.4.1 \
    rxjs@~6.3.3 \
    passport-local@^1.0.0 \
    path@^0.12.7 \
    tslib@^1.9.0 \
    mysql@^2.16.0 \
    pug@^2.0.3 \
    azure-storage@^2.10.2 \
    cookie-parser@^1.4.4 \
    express-validator@^5.3.1 \
    compression@^1.7.3 \
    jsonwebtoken@^8.5.0 \
    body-parser@^1.18.2 \
    nodemon@^1.18.10 \
    cross-env@^5.2.0 \
    express@4.16.4 \
    react-dom@^16.8.3 \
    helmet@^3.15.0 \
    react-dom@^16.8.4 \
    moment-timezone@^0.5.23 \
    react@^16.8.4 \
    mongodb@^3.1.13 \
    graphql@^14.1.1 \
    @sendgrid/mail@^6.3.1 \
    react@^16.8.3 \
    font-awesome@^4.7.0 \
    @angular/common@~7.2.0 \
    @angular/router@~7.2.0 \
    @angular/compiler@~7.2.0 \
    nodemailer@^5.1.1 \
    @angular/core@~7.2.0 \
    @angular/forms@~7.2.0 \
    @angular/platform-browser@~7.2.0 \
    @angular/platform-browser-dynamic@~7.2.0 \
    bcrypt-nodejs@0.0.3 \
    redux-thunk@^2.3.0 \
    react-scripts@2.1.5 \
    jsonwebtoken@^8.3.0 \
    helmet@^3.15.1 \
    bootstrap@^4.1.3 \
    bootstrap@^4.3.1 \
    jwt-decode@^2.2.0 \
    redux@^4.0.1 \
    serve-favicon@^2.5.0 \
    react-dom@^16.7.0 \
    react@^16.7.0 \
    morgan@^1.9.0 \
    debug@^3.2.6 \
    ejs@~2.5.7 \
    passport-azure-ad@^4.0.0 \
    mssql@^4.3.0 \
    ioredis@^3.2.2 \
    fs@0.0.1-security \
    cors@^2.8.4 \
; do
    installPackage $pkg
done;

# Cache some dev-dependencies
for pkg in \
    autoprefixer@6.3.6 \
    babel@6.5.2 \
    babel-core@6.24.0 \
    babel-eslint@7.2.3 \
    babel-loader@6.2.4 \
    babel-preset-es2015@6.6.0 \
    babel-preset-react@6.5.0 \
    css-loader@0.23.1 \
    del@2.2.0 \
    extract-text-webpack-plugin@1.0.1 \
    file-loader@0.8.5 \
    gulp@3.9.1 \
    gulp-eslint@2.0.0 \
    lodash@4.15.0 \
    style-loader@0.13.1 \
    url-loader@0.5.7 \
    webpack@1.12.14 \
; do
    installPackage $pkg
done;

echo "Caching done. Total cache size: $(du -sh $YARN_CACHE_DIR | cut -f -1)"
rm -fr $TEMP_APP_DIR

mkdir -p /output
cd $YARN_CACHE_DIR
tar -zcf /output/yarncache.tar.gz .
cd ..
rm -rf $YARN_CACHE_DIR
