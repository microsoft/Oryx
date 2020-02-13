# GitHub Action for generating a Dockerfile to build and run Azure Web Apps

With the Azure App Service Actions for GitHub, you can automate your workflow to deploy [Azure Web Apps](https://azure.microsoft.com/en-us/services/app-service/web/) using GitHub Actions.

Get started today with a [free Azure account](https://azure.com/free/open-source)!

This repository contains the GitHub Action for [generating a Dockerfile to build and run Azure Web Apps](./action.yml) using the [Oryx](https://github.com/microsoft/Oryx) build system. Currently, the following platforms can be built using this GitHub Action:

- .NET Core
- Node
- PHP
- Python

The generated Dockerfile follows a template similar to the following:

```
ARG RUNTIME=<PLATFORM_NAME>:<PLATFORM_VERSION>

FROM mcr.microsoft.com/oryx/build:<BUILD_TAG> as build
WORKDIR /app
COPY . .
RUN oryx build /app

FROM mcr.microsoft.com/oryx/${RUNTIME}
COPY --from=build /app /app
RUN cd /app && oryx
ENTRYPOINT ["/app/run.sh"]
```

Once this Dockerfile is produced, it can be built and pushed to a registry, such as [Azure Container Registry](https://azure.microsoft.com/en-us/services/container-registry/), and ran at a later time to deploy the Azure Web App.

If you are looking for a GitHub Action to build your Azure Web App, consider using [`azure/appservice-build`](https://github.com/Azure/appservice-build).

If you are looking for a GitHub Action to deploy your Azure Web App, consider using [`azure/webapps-deploy`](https://github.com/Azure/webapps-deploy).

The definition of this GitHub Action is in [`action.yml`](./action.yml).

# End-to-End Sample Workflows

## Dependencies on other GitHub Actions

- [`actions/checkout`](https://github.com/actions/checkout)
  - Checkout your Git repository content into the GitHub Actions agent

## Other GitHub Actions

- [`azure/login`](https://github.com/Azure/login)
  - Authenticate the current workflow to build, test, package, release and deploy to Azure
- [`azure/docker-login`](https://github.com/Azure/docker-login)
  - Log in to a private container registry, such as [Azure Container registry](https://azure.microsoft.com/en-us/services/container-registry/)
- [`azure/webapps-container-deploy`](https://github.com/Azure/webapps-container-deploy)
  - Deploy a web app container to Azure

### Sample workflow to push an image using Azure CLI

The following is an end-to-end sample of generating the Dockerfile, building the image, and pushing it to Azure Container Registry using Azure CLI whenever a commit is pushed:

```
on: push

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    steps:
      - name: Cloning repository
        uses: actions/checkout@v1

      - name: Running Oryx to generate a Dockerfile
        uses: microsoft/oryx/actions/oryx-dockerfile@master
        id: oryx

      - name: Azure authentication
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Building image and pushing to ACR
        run: |
          az acr build -t <IMAGE_NAME>:<TAG> \
                      -r <ACR_NAME> \
                      -f {{ steps.oryx.outputs.dockerfile-path }} \
                      .
```

The following variables should be replaced in your workflow `.yaml` file:

- `<ACR_NAME>`
    - Name of the Azure Container Registry that you are pushing to
- `<IMAGE_NAME>`
    - Name of the image that will be pushed to your registry
- `<TAG>`
    - Name of the image tag

The following variables should be set in the GitHub repository's secrets store:

- `AZURE_CREDENTIALS`
    - Used to authenticate calls to Azure; for more information on setting this secret, please see the [`azure/actions/login`](https://github.com/Azure/actions) action

### Sample workflow to push an image using Docker

The following is an end-to-end sample of generating the Dockerfile, building the image, and pushing it to a registry using Docker whenever a commit is pushed:

```
on: push

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    steps:
      - name: Cloning repository
        uses: actions/checkout@v1

      - name: Running Oryx to generate a Dockerfile
        uses: microsoft/oryx/actions/oryx-dockerfile@master
        id: oryx

      - name: Logging into registry
        uses: azure/docker-login@master
        with:
          login-server: <REGISTRY_NAME>
          username: ${{ secrets.REGISTRY_USERNAME }}
          password: ${{ secrets.REGISTRY_PASSWORD }}

      - name: Building image and pushing to registry using Docker
        run: |
          docker build . -t <REGISTRY_NAME>/<IMAGE_NAME>:<TAG> -f {{ steps.oryx.outputs.dockerfile-path }}
          docker push <REGISTRY_NAME>/<IMAGE_NAME>:<TAG>

```

The following variables should be replaced in your workflow:

- `<REGISTRY_NAME>`
    - Name of the registry that you are pushing to
- `<IMAGE_NAME>`
    - Name of the image that will be pushed to your registry
- `<TAG>`
    - Name of the image tag

The following variables should be set in the GitHub repository's secrets store:

- `REGISTRY_USERNAME`
    - The username for the container registry; for more information on setting this secret, please see the [`azure/container-actions/docker-login`](https://github.com/Azure/container-actions) action
- `REGISTRY_PASSWORD`
    - The password for the container registry; for more information on setting this secret, please see the [`azure/container-actions/docker-login`](https://github.com/Azure/container-actions) action

### Sample workflow to deploy an Azure Web App Container

The following is an end-to-end sample of generating the Dockerfile, building the image, pushing it to a registry using Docker, and deploying the web app to Azure whenever a commit is pushed:

```
on: push

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Cloning repository
        uses: actions/checkout@v1

      - name: Running Oryx to generate a Dockerfile
        uses: microsoft/oryx/actions/oryx-dockerfile@master
        id: oryx

      - name: Logging into Azure
        uses: azure/login@master
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Logging into registry
        uses: azure/docker-login@master
        with:
          login-server: <REGISTRY_NAME>
          username: ${{ secrets.REGISTRY_USERNAME }}
          password: ${{ secrets.REGISTRY_PASSWORD }}

      - name: Building image and pushing to registry using Docker
        run: |
          docker build . -t <REGISTRY_NAME>/<IMAGE_NAME>:<TAG> -f {{ steps.oryx.outputs.dockerfile-path }}
          docker push <REGISTRY_NAME>/<IMAGE_NAME>:<TAG>

      - name: Deploying container web app to Azure
        uses: azure/webapps-container-deploy@v1
        with:
          app-name: <WEB_APP_NAME>
          images: <REGISTRY_NAME>/<IMAGE_NAME>:<TAG>
```

The following variables should be replaced in your workflow:

- `<REGISTRY_NAME>`
    - Name of the registry that you are pushing to
- `<IMAGE_NAME>`
    - Name of the image that will be pushed to your registry
- `<TAG>`
    - Name of the image tag
- `<WEB_APP_NAME>`
    - Name of the web app that's being deployed

The following variables should be set in the GitHub repository's secrets store:

- `AZURE_CREDENTIALS`
    - Used to authenticate calls to Azure; for more information on setting this secret, please see the [`azure/actions/login`](https://github.com/Azure/actions) action
- `REGISTRY_USERNAME`
    - The username for the container registry; for more information on setting this secret, please see the [`azure/container-actions/docker-login`](https://github.com/Azure/container-actions) action
- `REGISTRY_PASSWORD`
    - The password for the container registry; for more information on setting this secret, please see the [`azure/container-actions/docker-login`](https://github.com/Azure/container-actions) action