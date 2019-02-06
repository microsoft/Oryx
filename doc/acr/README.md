# Use ACR with Oryx

You can use Oryx to build images for ACR from source. If you just want to
build once you don't even need to check in a file. If you want to set up
continuous build on commits you'll need to commit a [generic Dockerfile][]
which will be copied into your repo.

[generic Dockerfile]: ./Dockerfile.oryx

## Build

To build an image from a repo once (see [build-for-registry.sh](./build-for-registry.sh)):

```bash
git clone https://github.com/Microsoft/Oryx ~/oryx
cd path/to/your/repo
cp "~/oryx/doc/acr/Dockerfile.oryx" "."

registry_name=your-registry-name
registry_group_name=your-registry-group-name
image_name=your-image-name
repo_url='.'  # or a GitHub URL
runtime=node-10.14

az acr build \
    --registry ${registry_name} \
    --resource-group ${registry_group_name} \
    --file Dockerfile.oryx \
    --build-arg "RUNTIME=${runtime}" \
    --image ${image_name} \
    ${repo_url}

rm "./Dockerfile.oryx"
```

To build an image from a repo on every push, using the same first 9 lines as
above (see [task-for-registry.sh](./task-for-registry.sh)):

```bash
# commit and push the generic Dockerfile to your repo
git add Dockerfile.oryx && git commit -m "add generic Oryx Dockerfile" && git push

task_name=image-builder
az acr task create \
    --name ${task_name} \
    --registry ${registry_name} \
    --resource-group ${registry_group_name} \
    --context ${repo_url} \
    --file ${dockerfile_path} \
    --commit-trigger-enabled true \
    --arg "RUNTIME=${runtime}"

# to run the task manually:
az acr task run --name ${task_name} --registry ${registry_name}
```

## Run

You can run the image from your repository as follows, typical options in brackets:

```bash
docker run [-it | --detach] [-p 8080:8080] "${registry_name}.azurecr.io/${image_name}"
```
