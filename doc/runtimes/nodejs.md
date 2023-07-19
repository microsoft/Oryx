This document describes how **Node.js** apps are detected and built. It
includes details on Node.js-specific components and configuration of build
and run images too.

# Contents

- [Contents](#contents)
- [Base image](#base-image)
- [Detect](#detect)
  - [Detect Node Monorepos](#detect-node-monorepos)
- [Build](#build)
  - [Build Node Monorepos](#build-node-monorepos)
  - [Package manager](#package-manager)
- [Run](#run)
- [Version support](#version-support)

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

## Detect Node Monorepos
When `ENABLE_NODE_MONOREPO_BUILD=true` is set up and following condition is met:

1. one of these files is found in the root of the repo:
    * lerna.json
    * lage.config.js

# Build

The following process is applied for each build.

1. Run custom command or script if specified by `PRE_BUILD_COMMAND` or `PRE_BUILD_SCRIPT_PATH`.
2. Run custom build command if specified by `CUSTOM_BUILD_COMMAND`. This will skip running any package install commands 
   (`npm install` or `yarn install`) that Oryx originally included.
3. Run custom run build command if specified by `RUN_BUILD_COMMAND`. This will not skip package install commands.
4. Run `npm install`, which includes npm `preinstall` and `postinstall` scripts and also installs devDependencies.
   The `--production` flag is added to the NPM install command if some production
   dependencies are detected in the `package.json` file and if the
   [`PRUNE_DEV_DEPENDENCIES`](https://github.com/microsoft/Oryx/blob/main/doc/configuration.md) setting is set.
5. Run `npm run build` if a `build` script is specified in your `package.json`.
6. Run `npm run build:azure` if a `build:azure` script is specified in your `package.json`.
7. Run custom command or script if specified by `POST_BUILD_COMMAND` or `POST_BUILD_SCRIPT_PATH`.

## Build Node Monorepos

The following process is applied for building monorepos:

1. Run custom command or script if specified by `PRE_BUILD_COMMAND` or `PRE_BUILD_SCRIPT_PATH`.
2. Run `lerna init`, `lerna bootstrap` and `lerna run build` if lerna.json file is detected.
3. Run `npm lage run build` or `yarn lage run build` if lage.config.js file is detected.
4. Run custom command or script if specified by `POST_BUILD_COMMAND` or `POST_BUILD_SCRIPT_PATH`.

> NOTE: As described in [npm docs][], scripts named `prebuild` and `postbuild`
will run before and after `build` respectively if specified; and `preinstall` and
`postinstall` will run before and after `install`.

[npm docs]: https://docs.npmjs.com/misc/scripts

## Package manager

The version of npm used to install dependencies and run npm scripts is the
one bundled with the specified Node.js version as listed
[here](https://nodejs.org/en/download/releases/).

If a `yarn.lock` file is found in your repo root, or if you specify a version 
spec for yarn in the engines field of your application's `package.json`, a 
stable or specified version of yarn will be used instead of npm.

Note that **installing packages globally is unsupported**, whether requested directly
by your app or by some pre/post install script of an included package. For example,
this will **not** work in your `package.json`:

```json
  "scripts" : {
    "preinstall" : "npm install -g somepackage"
  }
```

# Run

The following process is applied to determine how to start an app.

1. Run `npm start` if a `start` script is specified.
1. Else, if a script is specified in `package.json`'s `main` field, run that.
1. Run the first found of the following scripts in the root of the repo:
    * bin/www
    * server.js
    * app.js
    * index.js
    * hostingstart.js

# Version support

<img width=500 src="https://raw.githubusercontent.com/nodejs/Release/master/schedule.svg?sanitize=true" />

The Node.js project follows the above [release schedule][]. Oryx will support
Long Term Stable (LTS) releases supported by the Node.js project and the
current release. Releases no longer supported by the upstream project will
eventually be removed from Oryx; even while they remain we may be constrained
in the support we can provide once upstream support ends.

We will provide notification twelve months prior to removing a release line;
subscribe to [Azure Updates][] to stay updated!

We will release updated versions of supported release lines at least
once every 3 months. Previous minor versions will remain available
as long as the release is available.

[release schedule]: https://github.com/nodejs/Release#release-schedule
[Azure Updates]: https://azure.microsoft.com/updates/
