# Use Oryx with a build service

Many build systems rely on a Dockerfile to describe how to build an app. You
can use Oryx with these systems by including and using the generic Dockerfile
in this directory: [Dockerfile.oryx][].

# Use Oryx with Azure Container Registry (ACR)

You can use Oryx within [Azure Container Registry (ACR) Tasks][] to build
container images directly from source without requiring your own Dockerfile.
Just copy the generic [Dockerfile.oryx][] to your repo and register a
one-time or recurring task with the following commands.

[Azure Container Registry (ACR) Tasks]: https://docs.microsoft.com/en-us/azure/container-registry/container-registry-tasks-overview
[Dockerfile.oryx]: https://github.com/microsoft/Oryx/blob/master/doc/hosts/Dockerfile.oryx

## Build once

Inject the Dockerfile and build your repo once.

```bash
registry_name=your-registry-name
image_name=your-image-name
repo_url='.'  # or an accessible git repo URL
runtime=node:10.14  # choose tags from https://mcr.microsoft.com/v2/oryx/node/tags/list


cd path/to/your/repo
curl -sSLO https://github.com/microsoft/Oryx/blob/master/doc/hosts/Dockerfile.oryx

az acr build \
    --registry ${registry_name} \
    --file Dockerfile.oryx \
    --build-arg "RUNTIME=${runtime}" \
    --image ${image_name} \
    ${repo_url}
```

## Build on commit

Commit the Dockerfile to your repo and schedule fresh builds on new commits.

```bash
registry_name=your-registry-name
image_name=your-image-name
repo_url=https://github.com/myorg/myrepo.git # cannot be `.` for recurring task
runtime=node:10.14  # choose from https://mcr.microsoft.com/v2/oryx/node/tags/list


cd path/to/your/repo
curl -sSLO https://github.com/microsoft/Oryx/blob/master/doc/hosts/Dockerfile.oryx

# commit and push the Oryx Dockerfile to your repo
git add Dockerfile.oryx && git commit -m "add generic Oryx Dockerfile" && git push

az acr task create \
    --name image-builder \
    --registry ${registry_name} \
    --context ${repo_url} \
    --file Dockerfile.oryx \
    --commit-trigger-enabled true \
    --arg "RUNTIME=${runtime}"

# to run the task manually:
az acr task run --name ${task_name} --registry ${registry_name}
```

## Run

You can run the image from your repository like any other image:

```bash
docker run "${registry_name}.azurecr.io/${image_name}"
```
