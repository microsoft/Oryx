This document describes how **Hugo** apps are detected and built. It includes
details on components and configuration of build images.

# Contents

- [Contents](#contents)
- [Detect](#detect)
- [Build](#build)
- [Version support](#version-support)

# Detect

The Hugo toolset is run when one of following conditions met:

1. `config.yaml`, `config.toml`, `config.yml` or `config.json` configuration files in root of repo.
2. Recursively look up config directory: `config/**/*.yaml`, `config/**/*.toml`,
   `config/**/*.yml` or `config/**/*.json` configuration files in sub-directories.

# Build

The following process is applied for each build:

1. Run custom script if specified by `PRE_BUILD_SCRIPT_PATH`.
2. Run custom script if specified by `POST_BUILD_SCRIPT_PATH`.

# Version support

The Hugo project defines this [release schedule][]. Oryx supports all actively supported
releases (v0.90.1).

We will update the `patch` version of a release at least once every 3 months,
replacing the previous `patch` version for that release.

[release schedule]: https://github.com/gohugoio/hugo/releases
