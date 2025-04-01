#!/bin/bash

if ! command -v yq &> /dev/null
then
    echo "yq could not be found, installing..."
    sudo apt-get update
    sudo apt-get install -y yq
fi

constants_yaml_file="./images/constants.yml"

# Get the absolute path
absolute_path=$(realpath "$constants_yaml_file")

# Print the absolute path to verify
echo "The absolute path to the YAML file is: $absolute_path"

declare -r REPO_DIR=$(cd images && pwd)

keys=$(yq e '.variables | keys' $constants_yaml_file | sed 's/^[- ]*//g')

for key in $keys; do
    value=$(yq e ".variables.$key" $constants_yaml_file)
    export $key=$value
done

# Update package list and install .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0

# build_image is githubactions, jamstack, cli
build_image=$1;
debian_flavor=$2;

set -ex


echo "current directory: $(pwd)"

dotnet publish ./src/BuildScriptGeneratorCli/BuildScriptGeneratorCli.csproj --configuration Release --output binaries --runtime linux-x64 --self-contained

case $build_image in
    "githubactions")
        case $debian_flavor in 
            "buster")
                docker build -f ./images/build/Dockerfiles/gitHubActions.Dockerfile -t githubactions_image_$debian_flavor --build-arg BASE_IMAGE=$GitHubActions_BaseImage_Buster --build-arg YARN_VERSION=$YARN_VERSION --build-arg YARN_MINOR_VERSION=$YARN_MINOR_VERSION --build-arg YARN_MAJOR_VERSION=$YARN_MAJOR_VERSION --build-arg DEBIAN_FLAVOR=$debian_flavor .
            ;;

            "bullseye")
                docker build -f ./images/build/Dockerfiles/gitHubActions.Dockerfile -t githubactions_image_$debian_flavor --build-arg BASE_IMAGE=$GitHubActions_BaseImage_Bullseye --build-arg YARN_VERSION=$YARN_VERSION --build-arg YARN_MINOR_VERSION=$YARN_MINOR_VERSION --build-arg YARN_MAJOR_VERSION=$YARN_MAJOR_VERSION --build-arg DEBIAN_FLAVOR=$debian_flavor .
            ;;

            "bookworm")
                docker build -f ./images/build/Dockerfiles/gitHubActions.Dockerfile -t githubactions_image_$debian_flavor --build-arg BASE_IMAGE=$GitHubActions_BaseImage_Bookworm --build-arg YARN_VERSION=$YARN_VERSION --build-arg YARN_MINOR_VERSION=$YARN_MINOR_VERSION --build-arg YARN_MAJOR_VERSION=$YARN_MAJOR_VERSION --build-arg DEBIAN_FLAVOR=$debian_flavor  --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING --build-arg SDK_STORAGE_BASE_URL_VALUE=$SDK_STORAGE_BASE_URL_VALUE .
            ;;
        esac
    ;;

    "jamstack")
        docker build -f ./images/build/Dockerfiles/cli.Dockerfile -t cli_image_$debian_flavor --build-arg DEBIAN_FLAVOR=$debian_flavor --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING --build-arg SDK_STORAGE_BASE_URL_VALUE=$SDK_STORAGE_BASE_URL_VALUE .
        docker build -f ./images/build/Dockerfiles/azureFunctions.JamStack.Dockerfile -t jamstack_image_$debian_flavor --build-arg PYTHON38_VERSION=$python38Version --build-arg DEBIAN_FLAVOR=$debian_flavor --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING --build-arg SDK_STORAGE_BASE_URL_VALUE=$SDK_STORAGE_BASE_URL_VALUE --build-arg BASE_IMAGE="docker.io/library/cli_image_$debian_flavor" --build-arg YARN_VERSION=$YARN_VERSION --build-arg YARN_MINOR_VERSION=$YARN_MINOR_VERSION --build-arg YARN_MAJOR_VERSION=$YARN_MAJOR_VERSION .
    ;;

    "cli")
        docker build -f ./images/build/Dockerfiles/cli.Dockerfile -t cli_image_$debian_flavor --build-arg DEBIAN_FLAVOR=$debian_flavor --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING --build-arg SDK_STORAGE_BASE_URL_VALUE=$SDK_STORAGE_BASE_URL_VALUE .
    ;;

esac



