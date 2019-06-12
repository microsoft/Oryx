# Oryx Buildpack

Oryx provides a buildpack that runs it, so that Oryx can also be used via the pack tool.

# Expected uses

# Motovation

[pack]: https://github.com/buildpack/pack

# Related images

WIP

# About the Oryx Project
We provide zero-config multi-language "source to web app" and "source to container" build tools.  Our tools are used across Microsoft tools to enable consistent and reliable builds of arbitrary source:

 * **[Azure AppService for Linux](https://github.com/Microsoft/Oryx/)** - Provides build capabilities for supported arbitrary languages
 * **[ACR Buildpack Tasks](https://github.com/microsoft/Oryx/blob/master/doc/buildpack.md)** - Brings buildpack support to Azure Container Registry (ACR) with the '[az acr pack](https://docs.microsoft.com/en-us/cli/azure/acr?view=azure-cli-latest#az-acr-pack)' commands
 * **Oryx Buildpack** - Cloud Native buildpack automatically builds from arbitrary source and generates OCI/Docker images 
 * **Oryx Images** - Base images with required tools for building supported langages and reduced size runtime images: [Docker Hub](https://hub.docker.com/_/microsoft-oryx-images), [Microsoft Container Registry (MCR)](https://azure.microsoft.com/en-us/blog/microsoft-syndicates-container-catalog/)

# Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md).

# License

MIT, see [LICENSE.md](./LICENSE.md).

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
