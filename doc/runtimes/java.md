This document describes how **Java** apps are detected and built. It includes
details on components and configuration of build images.

# Contents

- [Contents](#contents)
- [Detect](#detect)
- [Build](#build)
  - [Package manager](#package-manager)
- [Version support](#version-support)

# Detect

The Java toolset is run when following conditions met:

1. `.jsp` or `.java` extension files is found in root of repo or sub-directories.

# Build

The following process is applied for each build:

1. Run custom script or command if specified by `PRE_BUILD_SCRIPT_PATH` or `PRE_BUILD_COMMAND`.
2. Run Maven wrapper commands (`./mvnw clean package` for creating package and `./mvnw clean compile` for compiling),
   if maven wrapper shell file `mvnw` or cmd file `mvnw.cmd` were detected.
3. Run Maven commands (`mvn clean package` for creating package and `mvn clean compile` for compiling),
   if maven pom file `pom.xml` were detected.
4. Run custom script or command if specified by `POST_BUILD_SCRIPT_PATH` or `POST_BUILD_COMMAND`.

## Package manager

The latest version of *Maven* is used to install dependencies.

# Version support

The Java project defines this [release schedule][]. Oryx supports all actively supported
releases (1.8.0, 9.0.4, 10.0.2, 11.0.8, 12.0.2, 13.0.2, 14.0.2, 17.0.2).

We will update the `patch` version of a release at least once every 3 months,
replacing the previous `patch` version for that release.

[release schedule]: https://github.com/AdoptOpenJDK/openjdk8-binaries/releases
