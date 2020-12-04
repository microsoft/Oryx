This document describes how **Ruby** apps are detected and built. It includes
details on components and configuration of build images.

# Contents

- [Contents](#contents)
- [Detect](#detect)
- [Build](#build)
  - [Package manager](#package-manager)

# Detect

The PHP toolset is run when following conditions met:

1. `Gemfile` file is found at root of repo.
2. `Gemfile` file not found, however `Gemfile.lock` or `config.ru` files and iis startup files are found at root of repo.

# Build

The following process is applied for each build:

1. Run custom script if specified by `PRE_BUILD_SCRIPT_PATH`.
2. Run `gem install bundler` comamnd to install bundler tool.
3. Run `bundle install` command.
4. Run custom script if specified by `POST_BUILD_SCRIPT_PATH`.

## Package manager

The latest version of *gem* is used to install dependencies.
