# Architecture

The Oryx system comprises *Build* and *Run* images. **Build** images include
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

#### Additional requirements for tests

- [az CLI](https://github.com/Azure/azure-cli)
- [kubectl](https://kubernetes.io/docs/tasks/tools/install-kubectl/)
  
> **NOTE:** You must set credentials for connecting to a Kubernetes cluster in
> `.kube/config` prior to running tests. If using Azure Kubernetes service,
> just run this command: `az aks get-credentials -g <resource group> -n
> <cluster>`.


# Go (Golang)

The startup script generator is written in Go to reduce storage space
required. Nevertheless you don't have to install Go to build this project
since it's available in the build containers.

Set the `GOPATH` variable to include the Oryx repo, e.g.
`GOPATH=$GOPATH:c:\src\oryx`. Since the applications are inside the `src`
folder there, Go should be able to find the packages and produce builds.
