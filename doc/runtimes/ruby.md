This document describes how **Ruby** apps are detected and built. It includes
details on components and configuration of build images.

# Contents

- [Contents](#contents)
- [Detect](#detect)
- [Build](#build)
  - [Package manager](#package-manager)
- [Version support](#version-support)

# Detect

The Ruby toolset is run when one of following conditions met:

1. `Gemfile` file is found in root of repo.
2. `Gemfile.lock` or `config.ru` files and iis startup files are found at root of repo.

# Build

The following process is applied for each build:

1. Run custom script if specified by `PRE_BUILD_SCRIPT_PATH`.
2. Run `gem install bundler` comamnd to install bundler tool.
3. Run `bundle install` command.
4. Run custom script if specified by `POST_BUILD_SCRIPT_PATH`.

## Package manager

The latest version of *gem* is used to install dependencies.

# Version support

The Ruby project defines this [release schedule][]. Oryx supports all actively supported
releases (2.5.8, 2.6.6, 2.7.1, 2.7.2, 3.0.0, 3.1.1).

We will update the `patch` version of a release at least once every 3 months,
replacing the previous `patch` version for that release.

[release schedule]: https://www.ruby-lang.org/en/downloads/branches/
