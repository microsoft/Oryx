# Build

The Oryx [build image][] layers on Docker's
[buildpack-deps][library/buildpack-deps:stable] image, itself layered on
Docker's basic [debian][library/debian:stable] image.
The build image copies some required platforms from separate intermediate images,
which can be built locally using the `build-buildimage-platforms.sh` script.

[build image]: https://hub.docker.com/r/microsoft/oryx-build
[library/buildpack-deps:stable]: https://hub.docker.com/_/buildpack-deps
[library/debian:stable]: https://hub.docker.com/_/debian

`buildpack-deps` includes development libraries for many common system
packages required at build-time. The following system packages are also added
to the Oryx image. If you require additional packages please [open an
issue][].

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

For instace, if there is a `package.json` file at the root of the repo but it is not in the format expected
by npm, Oryx build will fail since it assumes that having that file in the repo's root means Node is
being used. There's also an option to disable _all_ platforms other than the one specified in `oryx build` 
or in AppService's language selection.
[Runtimes](./runtimes) have more information on how each platform is detected.

# Run

Oryx's run images build on Docker's runtime-specific images for [Node.js][]
and [Python][]. A start script generator is included. Packages included for
specific runtimes are described in their documentation.

Runtimes and versions supported by Oryx are listed in [the main
README](../README.md#supported-runtimes).

[Node.js]: https://github.com/nodejs/docker-node
[Python]: https://github.com/docker-library/python

# Azure Web Apps runtimes and versions

Runtime types and versions in Azure Web Apps are not necessarily the same as
those supported by Oryx.  Web App runtimes and versions can be listed with `az
webapp list-runtimes --linux`.

The ultimate type and version of your app's runtime in App Service is
determined by the value of `LinuxFxVersion` in your [site config][]. Set the
type and version during app creation through the `--runtime` parameter of `az
webapp create`, e.g. `az webapp create --name my-app --runtime 'NODE|10.14'`.

Check the current configured runtime type and version with `az webapp config
show ...`.

Change the runtime type and version with the following script:

```bash
app_name="your_app_name"
app_group="your_app_group"
web_id=$(az webapp show \
    --name "$app_name" \
    --resource-group "$app_group" \
    --output tsv --query id)

runtime='NODE|10.14'
az webapp config set \
    --ids $web_id \
    --linux-fx-version "$runtime"
```

[site config]: https://docs.microsoft.com/en-us/rest/api/appservice/webapps/get#siteconfig
