## Oryx Builder

### Steps for releasing new builder

#### Prerequisites

You must have access to the `oryxprodmcr` ACR instance in the Oryx production subscription account and have logged in
locally to `docker` with this ACR instance's credentials.

#### Build CLI image

_Note_: The CLI image used is built from the `corm/builder-skeleton-image` branch in the Oryx repository to avoid using
the current CLI image that has components dynamically installed via `apt-get` at run-time.

```
./oryx/build/buildBuildImages.sh -t cli-buster

docker tag oryx/cli-buster:debian-buster oryxprodmcr.azurecr.io/public/oryx/cli:builder-debian-buster-{BUILD_ID}
```

#### Push CLI image

```
docker push oryxprodmcr.azurecr.io/public/oryx/cli:builder-debian-buster-{BUILD_ID}
```

#### Update the stack Dockerfile with the new CLI tag

Open `oryx-builder/stack/Dockerfile` and update the CLI image used as a base with the new tag for the CLI image previously pushed.

_Note_: this should continue to point to MCR as the CLI image pushed to oryxprodmcr will be propagated to MCR shortly after.

#### Create stack images

```
cd .\oryx-builder\stack
docker build . -t oryxprodmcr.azurecr.io/public/oryx/builder:stack-base-{BUILD_ID} --target base
docker build . -t oryxprodmcr.azurecr.io/public/oryx/builder:stack-run-{BUILD_ID} --target run
docker build . -t oryxprodmcr.azurecr.io/public/oryx/builder:stack-build-{BUILD_ID} --target build
```

#### Push stack images

```
docker push oryxprodmcr.azurecr.io/public/oryx/builder:stack-base-{BUILD_ID}
docker push oryxprodmcr.azurecr.io/public/oryx/builder:stack-run-{BUILD_ID}
docker push oryxprodmcr.azurecr.io/public/oryx/builder:stack-build-{BUILD_ID}
```

#### Create buildpack image

```
cd .\oryx-builder\packaged-buildpack
pack buildpack package oryxprodmcr.azurecr.io/public/oryx/builder:buildpack-{BUILD_ID} --config .\package.toml
```

#### Push buildpack image

```
docker push oryxprodmcr.azurecr.io/public/oryx/builder:buildpack-{BUILD_ID}
```

#### Update the builder.toml with the new buildpack and stack tags

Open `oryx-builder/builder/builder.toml` and update the the buildpack and stack images used with the new tag previously pushed.

_Note_: these images should continue to point to MCR as the images pushed to oryxprodmcr will be propagated to MCR shortly after.

#### Create builder image

```
cd .\oryx-builder\builder
pack builder create oryxprodmcr.azurecr.io/public/oryx/builder:{BUILD_ID} --config .\builder.toml
```

#### Test builder image

```
pack build {YOUR_TEST_ACR}.azurecr.io/container-app:1234 --path .\oryx\tests\SampleApps\DotNetCore\NetCore6PreviewMvcApp --builder oryxprodmcr.azurecr.io/public/oryx/builder:{BUILD_ID} --run-image mcr.microsoft.com/oryx/dotnetcore:6.0 --env "CALLER_ID=test"
```

#### Push builder image

```
docker push oryxprodmcr.azurecr.io/public/oryx/builder:{BUILD_ID}
```