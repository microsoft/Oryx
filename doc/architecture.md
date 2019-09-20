# Architecture

The Oryx system comprises *Build* and *Run* images. **Build** images include
compilers, libraries, headers and other tools necessary to prepare artifacts;
**Run** images are smaller and contain only components required to run
programs.

After the generated build script runs, artifacts are exported as files from
the Build image to be mounted into the Run image or added to a derived
container in a Dockerfile.

Another script generator in Run images generates a script to start the app
correctly.

## Components

Follows a description of each of our components.

### Build script generator

The build script generator is a command line tool (and .NET Core library) that can generate and execute build
scripts for a given source repo.
It analyzes the codebase, detecting which programming platforms are being used and how each should be built.

### Build image

We have a single build image which supports all of the SDKs and their versions. This allows developers to use 
multiple languages in their build, for instance run a Python setup script when building their .NET Core app, 
or have a TypeScript frontend for their Python app. You can take a look at its 
[Dockerfile](../images/build/Dockerfile) to better understand its contents.

Note that some layers of this build image come from yet another set of images, which we build independently for
modularization and for faster build times. You can see what are those images and how they are built in their
[build script](../build/buildBuildImageBases.sh).

To help the user select which version they want for each platform, they can use the `benv` script pre-installed
in the build image. For example, `source benv python=3.6 node=8` will make Python 3.6 and the latest supported
version of Node 8 the default ones.

The build image also contains the build script generator, which can be accessed by its alias, `oryx`.

The build image manifest is at
[/images/build/Dockerfile](../images/build/Dockerfile). It is built and
published via the Microsoft Container Registry (MCR) ([info][]) as
`mcr.microsoft.com/oryx/build` and syndicated to Docker Hub as
[`https://hub.docker.com/_/microsoft-oryx-images`](https://hub.docker.com/_/microsoft-oryx-images). Pull with `docker pull
mcr.microsoft.com/oryx/build:latest`.

[info]: https://azure.microsoft.com/en-us/blog/microsoft-syndicates-container-catalog/

### Startup script generators

These are command line tools, one for each platform, that inspect the output directory of an application and
write a script that can start it. They are written in Go, and are located in
[src/startupscriptgenerator](../src/startupscriptgenerator/).

The startup script generators are written in Go to reduce storage space
required. Nevertheless you don't have to install Go to build this project
since it's available in the Oryx build image.

Set the `GOPATH` variable to include the Oryx repo, for example
`GOPATH=$GOPATH:c:\src\oryx`. Since the applications are inside the `src`
folder there, Go should be able to find the packages and produce builds.

### Run images

We have a set of runtime images, and their Dockerfiles are located in [/images/runtime](../images/runtime).
Some of the Dockerfiles are generated from a template, also located in this folder, with a corresponding 
script to turn those scripts into actual Dockerfiles. Having templates helps us maintain consistency
across the Dockerfiles.

There are some exceptions that are not templates, where we have to customize the image. A typical need for
such customization is security, where we have to patch a tool or rely on a different base image than the
official images released for a given platform.

Each runtime image contains the startup script generator for its platform.

The *Run* images are published to MCR (mcr.microsoft.com/oryx/&lt;platform&gt;).


## Repo structure

* `build`: scripts for building the script generator and build and runtime images
* `images`: Dockerfiles for the build and runtime images
* `src`: source code for the build and startup script generators
* `tests`: tests.
* `vsts`: CI/CD configuration.

## Prerequisites

The following are required to run and test this project locally.

- Bash v4.4
- [.NET Core 2.1](https://dotnet.microsoft.com/download/dotnet-core)
- [Go 1.11+](https://golang.org/dl/) (for startup script generator)
- [Docker v18.06.1-ce](https://docs.docker.com/install/)
