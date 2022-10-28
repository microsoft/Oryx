# Build

The Oryx [build image][] layers on Docker's
[`buildpack-deps`][library/buildpack-deps:stable] image, itself layered on
Docker's basic [debian][library/debian:stable] image.
The build image copies some required platforms from separate intermediate images,
which can be built locally using the [`buildBuildImageBases.sh`](../build/buildBuildImageBases.sh) script.

[build image]: https://hub.docker.com/_/microsoft-oryx-images
[library/buildpack-deps:stable]: https://hub.docker.com/_/buildpack-deps
[library/debian:stable]: https://hub.docker.com/_/debian

`buildpack-deps` includes development libraries for many common system
packages required at build-time. The following system packages are also added
to the Oryx image. If you require additional packages please [open an
issue][].

Support different version of debian (stretch, buster and bullseye) and ubuntu (focal) `buildpack-deps` based images:
[Buster](../images/build/Dockerfiles/githubRunner.BuildPackDepsBuster.Dockerfile),
[Ubuntu(focal)](../images/build/Dockerfiles/githubRunner.BuildPackDepsFocal.Dockerfile),
[Stretch](../images/build/Dockerfiles/githubRunner.BuildPackDepsStretch.Dockerfile),
[Bullseye](../images/build/Dockerfiles/githubRunner.BuildPackDepsBullseye.Dockerfile)

[open an issue]: https://github.com/Microsoft/Oryx/issues/new/choose

* jq
* libgssapi-krb5-2 (for dotnetcore)
* libicu57 (for dotnetcore)
* liblttng-ust0 (for dotnetcore)
* libstdc++6 (for dotnetcore)
* unixodbc-dev (for MSSQL for Python)
* tk-dev (for Python)
* uuid-dev (for Python)

When building the application, Oryx tries to detect every programming platform being used
and for each one that it finds, it adds the corresponding commands to the build script. If the
user wants to disable a specific programming platform, there are a set of environment variables
that can be set - please refer to [Oryx configuration](./configuration.md#oryx-configuration).

For instance, if there is a `package.json` file at the root of the repo but it is not in the format expected
by npm, Oryx build will fail since it assumes that having that file in the repo's root means Node is
being used. There's also an option to disable _all_ platforms other than the one specified in `oryx build` 
or in AppService's language selection.
[Runtimes](./runtimes) have more information on how each platform is detected.

# Run

Oryx's run images build on Docker's runtime-specific images for [Node.js][]
, [Python][], [Php][] and [Dotnet][]. A start script generator is included. Packages included for
specific runtimes are described in their documentation.
The runtime image can be built locally using the [`buildRunTimeImage.sh`](../build/buildRunTimeImage.sh) script.

Runtimes and versions supported by Oryx are listed in [the main
README](../README.md#supported-platforms).

[Node.js]: https://github.com/nodejs/docker-node
[Python]: https://github.com/docker-library/python
[Php]: https://github.com/docker-library/php
[Dotnet]: https://github.com/dotnet/dotnet-docker
