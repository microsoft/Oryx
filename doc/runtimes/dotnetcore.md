This document describes how **.NET Core** apps are detected and built. It
includes details on .NET Core-specific components and configuration of build
and run images too.

# Contents

1. [Base image](#base-image)
1. [Detect](#detect)
1. [Build](#build)
1. [Run](#run)
1. [Version support](#version-support)

# Base image

.NET Core runtime images are built on official [microsoft/dotnet][] images from
the Microsoft Container Registry.

[microsoft/dotnet]: https://hub.docker.com/_/microsoft-dotnet-core

# Detect

The .NET Core toolset is applied when a `.csproj` ("C# Project") file is found
in the root or a subdirectory of the repo. If a `.csproj` file is found in the
root it is used; if not in the root but exactly one `.csproj` file is found in
a subdirectory, it is used. If multiple files are found in subdirectories an
error is returned.

The `PROJECT` environment variable (App Setting in App Service) can be used to
specify a repo-relative path to a valid `.csproj` file to manage the build. If
this variable is specified no other checks are carried out.

Only ASP.NET Core apps are supported; therefore the .csproj file must include a
reference to the `Microsoft.AspNetCore` assembly.

# Build

The following process is applied for each build.

1. Run custom script if specified by `PRE_BUILD_SCRIPT_PATH`.
1. Run `dotnet restore` to restore Nuget dependencies.
1. Run `dotnet publish` to build a binary for production.
1. Run custom script if specified by `POST_BUILD_SCRIPT_PATH`.

# Run

By default, your app is run based on the AssemblyName specified in the .csproj
file. If an AssemblyName is not specified, the name of the .csproj file without
extension is used. For example, if your app includes a file
`dotnet-react.csproj`, it will be run with `dotnet run dotnet-react.dll`.

The startup command can be set manually as described in
[../configuration.md#startup-file](../configuration.md#startup=file).

# Version support

.NET Core follows a [release support policy][] which includes Long-Term Support
(LTS) and Current releases.  Oryx will support LTS releases supported by the
.NET Core project and the Current release. Releases no longer supported by the
upstream project will eventually be removed from Oryx; even while they remain
we may be constrained in the support we can provide once upstream support ends.

We will provide notification twelve months prior to removing a release line;
subscribe to [Azure Updates][] to stay updated!

We will release updated versions of supported release lines at least
once every 3 months.

[release support policy]: https://dotnet.microsoft.com/platform/support/policy/dotnet-core
[Azure Updates]: https://azure.microsoft.com/updates/
