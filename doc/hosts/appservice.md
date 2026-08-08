# Azure App Service for Linux

In [Azure App Service on Linux][] the user has many options to deploy their application, including 
[continuous deployment][] - in which the source code is uploaded to App Service that in turn builds
the application. If no custom deployment script is provided, Oryx will run behind the scenes and perform
the required steps to build and configure the application. 

Even if the application is built outside of App Service, for example through an external CI/CD pipeline,
Oryx is still invoked to detect how to start an application if no start up command was specified.

Here we describe some details of this process, and how you might configure them and fix issues
if needed. We focus on the specifics for App Service; for how we support each language/runtime in
general, please refer to their specific entry in our [docs page](../README.md).

[Azure App Service on Linux]: https://docs.microsoft.com/en-us/azure/app-service/containers/app-service-linux-intro
[continuous deployment]: https://docs.microsoft.com/en-us/azure/app-service/deploy-continuous-deployment?toc=%2fazure%2fapp-service%2fcontainers%2ftoc.json

## Overview

When an application is pushed to App Service, for example through local git or GitHub integration, the
source code will be available in `/home/site/repository`. Through a hook in the git repository,
after the code is pushed App Service will call Oryx to build the application if a deployment
script wasn't provided. After the build step the web app is placed in `/home/site/wwwroot`,
location from which it will be executed.

App Service has two components that are relevant in this discussion: the [Kudu][] service,
where the build happens, and the runtime environments where the web apps are executed. Both environments
share files through the `/home` network directory.

As a side note, since `/home` is a shared location for both build and runtime, and is also a 
persistent storage location, some web apps use it to store state. We don't recommend this since
it's easy to run into concurrent access issues. We recommend the use of services specifically
designed for [storage][], which might also include backups, replication, and much more.

[Kudu]: https://github.com/azure-App-Service/kudulite
[storage]: https://azure.microsoft.com/en-us/product-categories/storage/

## Configuration

Within Azure App Service, Oryx's environment variables are set via [App
Settings][].

List and modify these App Settings with the [az CLI][] using the following
commands:

```bash
app_group=your-group
app_name=your-app

# list current settings
az webapp config appsettings list \
  --resource-group $app_group --name $app_name \
  --output table

# replace current settings
az webapp config appsettings set \
    --resource-group $app_group --name $app_name \
    --settings \
      "settingA=${settingA}" \
      "settingB=${settingB}"
```

App Service adds the following settings that govern build:

Setting name                        | Description                                                    | Example
------------------------------------|----------------------------------------------------------------|------------
COMMAND                             | provide an alternate build-and-run script. Bypasses automatic build completely. | "repo/path/to/script.sh"
ENABLE\_ORYX\_BUILD                 | if `true`, use the Oryx build system instead of the legacy Kudu system | 
SCM\_DO\_BUILD\_DURING\_DEPLOYMENT` | if `false`, bypass automatic build |

For the complete list of configuration options available, including how to enable
multiple platforms, please check our [configuration page](../configuration.md).

### Startup file

Within App Service, to explicitly specify a start script use the
`--startup-file` parameter of `az webapp create ...` or `az webapp config set
...`.

[App Settings]: https://docs.microsoft.com/en-us/azure/app-service/web-sites-configure#app-settings
[az CLI]: https://github.com/Azure/azure-cli

## Runtimes and versions

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

## Node.js

In general, Node.js applications have a large number of direct and indirect dependencies, that is, 
the packages it uses and their dependencies. Each package might contain several `.js` files, thus fetching
dependencies means a lot of disk I/O operations. Since in the App Service model the application is stored in a 
network volume, the `/home` directory, fetching and storing the packages alongside the application in 
`/home/site/wwwroot` means a lot of I/O operations would have to go through the network. 

### Compressing Node.js modules

Recently we've made some build performance improvements that made the builds run up to ten times faster. We achieved
this by fetching the packages outside of the network location, compress its contents, and maintain all the dependencies
in a single file, `node_modules.tar.gz`, that is located in `/home/site/wwwroot`. As a result, we no longer maintain the
`node_modules` folder in this `/home/site/wwwroot`.

As part of the application startup process, we extract the contents of `node_modules.tar.gz` to `/node_modules`, which
is outside of the volume share. Since the Node.js runtime looks for packages inside directories called `node_modules`
starting at the application directory (`/home/site/wwwroot`) all the way to `/`, it is able to find the extracted
packages.

If for some reason you want to disable this behavior, for example there are hardcoded references to files inside 
`node_modules`, you can set the app setting `BUILD_FLAGS` to `Off`. Note that casing matters, so `off` won't work.
This flag will disable the performance optimizations and put the `node_modules` directory back inside the application's
directory in the network volume.

## .NET Core

The .NET Core DLL files are located in `/home/site/wwwroot`, and since this is a shared location, concurrent access
issues might happen. For instance, if you updated your application and don't see your changes reflected when you
use it, set the application setting `APP_SVC_RUN_FROM_COPY` to `true`. This will make the application run from a
location other than `/home/site/wwwroot`. 

When using this solution, you should not have hardcoded references to files that include `/home/site/wwwroot` in 
the path, for example in custom startup scripts. Alternatively, you can just reference those files by its relative
path, removing `/home/site/wwwroot` from it.

## Network Dependencies

When using App Service with a Virtual Network or an App Service Environment, you will need to allow outbound access 
from the webapp to `oryx-cdn.microsoft.io` on port `443`. `oryx-cdn.microsoft.io` hosts the Oryx packages corresponding
to each SDK language and version. If this network dependency is blocked, then App Service will not be able to build your 
application using Oryx.
