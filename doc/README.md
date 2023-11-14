These documents describe components and configuration of Oryx's build and run
system and how to configure or change them.

# Contents

1. Using Oryx
    * Using [Azure App Service](./hosts/appservice.md) for Linux, where Oryx is called to build and run web apps.
    * [Custom Dockerfile](./hosts/Dockerfile.md) that calls Oryx to build and run the application.
1. [Configure](./configuration.md) specific features, including support for multiple platforms.
1. [Runtimes](./runtimes)
    * [Node.js](./runtimes/nodejs.md)
    * [Python](./runtimes/python.md)
    * [.NET Core](./runtimes/dotnetcore.md)
    * [PHP](./runtimes/php.md)
    * [Java](./runtimes/java.md)
    * [Hugo](./runtimes/hugo.md)
1. [Architecture and components](./architecture.md)
1. [Build](./base_images.md#build) image, which contains the supported SDKs.
1. [Runtime](./base_images.md#run) images, streamlined to run applications previously built using the build image.
    * [Azure Web Apps runtimes and versions](./hosts/appservice.md#runtimes-and-versions)
1. [Supported platform versions](./supportedPlatformVersions.md)
