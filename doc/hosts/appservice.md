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
persistent storage location, some web apps use it to store state. We don't recomment this since
it's easy to run into concurrent access issues. We recommend the use of services specifically
designed for [storage][], which might also include backups, replication, and much more.

[Kudu]: https://github.com/azure-App-Service/kudulite
[storage]: https://azure.microsoft.com/en-us/product-categories/storage/

## Node.js

In general, Node.js applications have a large number of package dependencies, either directly or indirectly,
that is, the dependencies of their dependencies. Since each package might contain several `.js` files, fetching
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
git lo