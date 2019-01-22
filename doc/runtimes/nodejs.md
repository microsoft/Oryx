This document describes how **Node.js** apps are detected and built. It
includes details on Node.js-specific components and configuration of build
and run images too.

# Contents

1. [Base image](#base-image)
1. [Detect](#detect)
1. [Build](#build)
    * [Package manager](#package-manager)
1. [Run](#run)

# Base image

Node.js runtime images are built on [the official Node.js
image](https://github.com/nodejs/docker-node).

# Detect

The Node.js toolset is run when the following conditions are met:

1. One of these files is found in the root of the repo:
    * package.json
    * package-lock.json
    * yarn.lock
1. One of these files is found in the root of the repo:
    * server.js
    * app.js

# Build

The following process is applied for each build.

1. Run custom script if specified by `PRE_BUILD_SCRIPT_PATH`.
1. Run `npm install` without any flags, which includes npm `preinstall` and
   `postinstall` scripts and also installs devDependencies.
1. Run `npm run build` if a `build` script is specified.
1. Run `npm run build:azure` if a `build:azure` script is specified.
1. Run custom script if specified by `POST_BUILD_SCRIPT_PATH`.

## Package manager

The version of npm used to install dependencies and run npm scripts is the
one bundled with the specified Node.js version as listed
[here](https://nodejs.org/en/download/releases/).

If a `yarn.lock` file is found in your repo root or if you specify "yarn" in
the `engines` field of package.json, the latest or specified version of yarn
will be used instead of npm.

# Run

The following process is applied to determine how to start an app.

1. Run `npm start` if a `start` script is specified.
1. If a script is specified in package.json's `main` field run that.
1. Run the first found of the following scripts in the root of the repo:
    * bin/www
    * server.js
    * app.js
    * index.js
    * hostingstart.js

In Azure Web Apps the version of the Node.js runtime which runs your app is
determined by the value of `LinuxFxVersion` in your [site config][]. See
[../base\_images.md](../base_images.md#azure-web-apps-runtimes-and-versions)
for how to modify this.

[site config]: https://docs.microsoft.com/en-us/rest/api/appservice/webapps/get#siteconfig
