# Oryx configuration

Oryx provides options to enable users to apply minor adjustments to its build
process. These options are configured by specially-named environment
variables which can be specified in an [env file][] named `build.env` in the
repo root.

These environment variables can also be specified as App Service [App
Settings][] in that environment; App Settings take precedence over variables
specified in a local file.

Only the names explicitly specified here are recognized and used.

* `PRE_BUILD_SCRIPT_PATH`: repo-relative path to a Bash script to run before
  build.
* `POST_BUILD_SCRIPT_PATH`: repo-relative path to a Bash script to run after
  build.

[env file]: https://docs.docker.com/compose/env-file/

# App Service configuration

App Service also defines variables to control aspects of build and run as
[documented here][Configurable settings]. These can be specified in an
ini-style [.deployment file][] in the repo root or as [App Settings][] on
the web app resource. Some important variables related to build are as
follows:

* `ENABLE_ORYX_BUILD`: if `true`, use the Oryx build system instead of the legacy Kudu system.
* `COMMAND`: alternate build script. Bypasses automatic build completely.
* `PROJECT`: alternate path to root of build directory.
* `SCM_POST_DEPLOYMENT_ACTIONS_PATH`: path to directory of scripts to be executed after deployment.
* `SCM_DO_BUILD_DURING_DEPLOYMENT`: if `false`, bypass automatic build.

[Configurable settings]: https://github.com/projectkudu/kudu/wiki/Configurable-settings
[.deployment file]: https://github.com/projectkudu/kudu/wiki/Custom-Deployment-Script
[App Settings]: https://docs.microsoft.com/en-us/azure/app-service/web-sites-configure#app-settings

## Startup file

To override Oryx's defaults for starting your app, specify a command or path
for the `--startup-file` parameter of `az webapp create ...` or `az webapp
config set ...`.

TODO: enable user-specified start file outside of App Service.