# Oryx Buildpack

Oryx provides a buildpack that runs it, so that Oryx can also be used via the Cloud Native `[pack][]` tool, with your own Azure Container Registry (ACR) using ACR Tasks with the Azure CLI `az acr pack` command.  

# Usage

```bash
### Setup ACR

$ az group create -n MyGroup
$ az acr create -n MyRegistry -g MyGroup --sku basic

### run buildpack with source pulled from current directory
### deploys container with ready to run web app

$ az acr pack -r MyRegistry -t sample-app .

### Cleanup

$ az group delete -n MyGroup
```

See the primary Oryx page for an up-to-date [list of languages supported](/README.md). 

# Motivation

At Microsoft, we're excited by the creation of the [Buildpacks](http://buildpacks.io) effort to standardize 'source to container' tools and resources.  Because Oryx provides zero-configuration 'source to web application' capability for Azure App Service for Linux, and Azure Container Registry supports OCI images, it's a natural fit to offer Oryx as a standalone buildpack.

# Related images

Information, including docker files for both build and run images, can be [found here](https://github.com/Microsoft/Oryx/tree/master/images/pack-builder).

# About the Oryx Project
We provide zero-config multi-language "source to web app" and "source to container" build tools.  Our tools are used across Microsoft projects and services to enable consistent and reliable builds of arbitrary source:

 * **[Azure AppService for Linux](/doc/appservice.md)** - Supports building and running websites written in various languages, directly from their source code
 * **[ACR Buildpack Tasks](/doc/buildpack.md)** - Brings buildpack support to Azure Container Registry (ACR) with the '[az acr pack](https://docs.microsoft.com/en-us/cli/azure/acr?view=azure-cli-latest#az-acr-pack)' commands
 * **[Oryx Buildpack](/doc/buildpack.md)** - Cloud Native buildpack automatically builds from arbitrary source and generates OCI/Docker images 
 * **Oryx Images** - Base images with required tools for building supported langages and reduced size runtime images: [Docker Hub](https://hub.docker.com/_/microsoft-oryx-images), [Microsoft Container Registry (MCR)](https://azure.microsoft.com/en-us/blog/microsoft-syndicates-container-catalog/)

# Contributing and License

For information on contributing and licensing, please visit our [readme](/README.md).

# Security

Security issues and bugs should be reported privately, via email, to the
Microsoft Security Response Center (MSRC) at
[secure@microsoft.com](mailto:secure@microsoft.com). You should receive a
response within 24 hours. If for some reason you do not, please follow up via
email to ensure we received your original message. Further information,
including the [MSRC
PGP](https://technet.microsoft.com/en-us/security/dn606155) key, can be found
in the [Security
TechCenter](https://technet.microsoft.com/en-us/security/default).

# Data/Telemetry

When utilized within Azure services, this project collects usage data and
sends it to Microsoft to help improve our products and services. Read
[Microsoft's privacy statement][] to learn more.

[Microsoft's privacy statement]: http://go.microsoft.com/fwlink/?LinkId=521839

This project follows the [Microsoft Open Source Code of Conduct][coc]. For
more information see the [Code of Conduct FAQ][cocfaq]. Contact
[opencode@microsoft.com][cocmail] with questions and comments.

[coc]: https://opensource.microsoft.com/codeofconduct/
[cocfaq]: https://opensource.microsoft.com/codeofconduct/faq/
[cocmail]: mailto:opencode@microsoft.com
[pack]: https://github.com/buildpack/pack
