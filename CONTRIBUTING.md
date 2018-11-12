We welcome all contributions to the Oryx project: documentation, bug reports,
feature requests and source code. Please submit these using GitHub issues and
pull requests.

To build and test the project, install the following prerequisites and run the
following scripts.

## Prerequisites

- bash v4.4
- dotnet v2.1
- docker v18.06.1-ce. This is the version used in CI too. We want a consistent
  version across development and CI to avoid surprises.

Run these scripts from the repo root:

- Build build image: `./build/build-buildimages.sh`
- Build runtime images: `./build/build-runtimeimages.sh`
- Build and test build image: `./build/test-buildimages.sh`
- Build and test runtime images: `./build/test-runtimeimages.sh`
- Build and test build and runtime images and other tests: `./build.sh`

## Repo Contents

* `build`: scripts for building the script generator and build and runtime
  images
* `images`: Dockerfiles for the build and runtime images
* `src`: source code for the script generator
