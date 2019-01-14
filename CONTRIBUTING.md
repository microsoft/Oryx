# Contents

1. [Contributing](#contributing)
1. [Architecture](#architecture)

# Contributing

We welcome all contributions to the Oryx project: questions and answers, bug
reports, feature requests and source code. Please submit these using GitHub
issues and pull requests.

Most contributions require you to agree to a Contributor License Agreement
(CLA) declaring that you have the right to, and actually do, grant us the
rights to use your contribution. For details, visit
<https://cla.microsoft.com>.

When you submit a pull request, a bot will determine whether you need to
provide a CLA and decorate the PR appropriately (e.g., label, comment).
Simply follow the instructions provided by the bot. You will only need to do
this once across all repositories using our CLA.

## Conduct

This project has adopted the [Microsoft Open Source Code of
Conduct](https://opensource.microsoft.com/codeofconduct/). For more
information see the [Code of Conduct
FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact
[opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional
questions or comments.

# Architecture

The system comprises *Build* and *Run* images. **Build** images include
compilers, libraries, headers and other tools necessary to prepare artifacts;
**Run** images are smaller and contain only components required to run
programs.

After the generated build script runs, artifacts are exported as files from
the Build image to be mounted into the Run image or added to a derived
container in a Dockerfile.

Another script generator in Run images generates a script to start the app
correctly.

## Repo structure

* `build`: scripts for building the script generator and build and runtime images
* `images`: Dockerfiles for the build and runtime images
* `src`: source code for the build and startup script generators
* `tests`: tests.
* `vsts`: CI/CD configuration.

## Script generators

A key element of the Build image is the
[BuildScriptGenerator](./src/BuildScriptGenerator), which analyzes the
codebase and prepares an appropriate build script. A key element of the Run
image is the [StartupScriptGenerator](./src/startupscriptgenerator), which
analyzes the codebase and prepares an appropriate start script.

## Images

The `build` image manifest is at
[./images/build/Dockerfile](./images/build/Dockerfile). It is built and
published via the Microsoft Container Registry (MCR) ([info][]) as
`mcr.microsoft.com/oryx/build` and syndicated to Docker Hub as
[index.docker.io/oryxprod/build:latest][]. Pull with `docker pull
mcr.microsoft.com/oryx/build:latest`.

[info]: https://azure.microsoft.com/en-us/blog/microsoft-syndicates-container-catalog/
[index.docker.io/oryxprod/build:latest]: https://hub.docker.com/r/oryxprod/build
[index.docker.io/microsoft/oryx-build]: https://hub.docker.com/r/microsoft/oryx-build

The *Run* images are defined in [`./images/runtime`](./images/runtime) and
published to MCR and Docker Hub as well at
<https://hub.docker.com/u/oryxprod>.

**TODO**: include a couple examples of pulling run images once repo location
is final.

## Prerequisites

The following are required to run and test this project locally.

- bash v4.4
- dotnet v2.1
- go 1.11+ (for startup script generator)
- docker v18.06.1-ce

#### Additional integration test prerequisites
- Azure CLI.
- kubectl. Can be installed using Azure CLI: `az aks install-cli`
- Credentials to an Azure Kubernetes Service cluster. To fetch: `az aks get-credentials -g <resource group> -n <cluster>`

# Go (Golang)

The startup script generator is written in Go to reduce storage space
required. Nevertheless you don't have to install Go to build this project
since it's available in the build containers.

Set the `GOPATH` variable to include the Oryx repo, e.g.
`GOPATH=$GOPATH:c:\src\oryx`. Since the applications are inside the `src`
folder there, Go should be able to find the packages and produce builds.