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

stack_name=$1;
stack_version=$2;
debian_flavor=$3;

set -ex

# declare -r REPO_DIR=$(cd images && pwd)

echo "Current directory: $(pwd)"

# source $REPO_DIR/defaultVersions.sh

docker build -f ./images/runtime/commonbase/Dockerfile -t oryx_run_base_$debian_flavor --build-arg DEBIAN_FLAVOR=$debian_flavor .

case $stack_name in
    "dotnet") 
        curl -SL --output "DotNetCoreAgent.$DotNetCoreAgent_version.zip" "https://oryxsdksdev.blob.core.windows.net/appinsights-agent/DotNetCoreAgent.$DotNetCoreAgent_version.zip"
       case $stack_version in
            "6.0")
                docker build -f ./images/runtime/dotnetcore/6.0/$debian_flavor.Dockerfile -t dotnet6_image_$debian_flavor --build-arg NET_CORE_APP_60_SHA=$NET_CORE_APP_60_SHA --build-arg ASPNET_CORE_APP_60_SHA=$ASPNET_CORE_APP_60_SHA --build-arg NET_CORE_APP_60=$NET_CORE_APP_60 --build-arg ASPNET_CORE_APP_60=$ASPNET_CORE_APP_60 --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
            ;;

            "7.0")
                docker build -f ./images/runtime/dotnetcore/7.0/$debian_flavor.Dockerfile -t dotnet7_image_$debian_flavor --build-arg NET_CORE_APP_70_SHA=$NET_CORE_APP_70_SHA --build-arg ASPNET_CORE_APP_70_SHA=$ASPNET_CORE_APP_70_SHA --build-arg NET_CORE_APP_70=$NET_CORE_APP_70 --build-arg ASPNET_CORE_APP_70=$ASPNET_CORE_APP_70 --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
            ;;

            "8.0")
                docker build -f ./images/runtime/dotnetcore/8.0/$debian_flavor.Dockerfile -t dotnet8_image_$debian_flavor --build-arg NET_CORE_APP_80_SHA=$NET_CORE_APP_80_SHA --build-arg ASPNET_CORE_APP_80_SHA=$ASPNET_CORE_APP_80_SHA --build-arg NET_CORE_APP_80=$NET_CORE_APP_80 --build-arg ASPNET_CORE_APP_80=$ASPNET_CORE_APP_80 --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
            ;;

            "9.0")
                docker build -f ./images/runtime/dotnetcore/9.0/$debian_flavor.Dockerfile -t dotnet9_image_$debian_flavor --build-arg NET_CORE_APP_90_SHA=$NET_CORE_APP_90_SHA --build-arg ASPNET_CORE_APP_90_SHA=$ASPNET_CORE_APP_90_SHA --build-arg NET_CORE_APP_90=$NET_CORE_APP_90 --build-arg ASPNET_CORE_APP_90=$ASPNET_CORE_APP_90 --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
            ;;
        esac

        rm -f ./DotNetCoreAgent.2.8.42.zip
    ;;

    "node")
        docker build -f ./images/runtime/commonbase/nodeRuntimeBase.Dockerfile -t oryx_node_run_base_$debian_flavor --build-arg BASE_IMAGE="docker.io/library/oryx_run_base_$debian_flavor" .
       case $stack_version in
            "18")
                curl -SL --output "nodejs-$debian_flavor-$node18Version.tar.gz" "https://oryxsdksdev.blob.core.windows.net/nodejs/nodejs-$debian_flavor-$node18Version.tar.gz"
                docker build -f ./images/runtime/node/18/$debian_flavor.Dockerfile -t node18_$debian_flavor --build-arg NODE18_VERSION=$node18Version --build-arg BASE_IMAGE="docker.io/library/oryx_node_run_base_$debian_flavor" --build-arg NPM_VERSION=$NPM_VERSION --build-arg PM2_VERSION=$PM2_VERSION --build-arg NODE_APP_INSIGHTS_SDK_VERSION=$NODE_APP_INSIGHTS_SDK_VERSION --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
                rm -f ./nodejs-$debian_flavor-$node18Version.tar.gz
            ;;

            "20")
                curl -SL --output "nodejs-$debian_flavor-$node20Version.tar.gz" "https://oryxsdksdev.blob.core.windows.net/nodejs/nodejs-$debian_flavor-$node20Version.tar.gz"
                docker build -f ./images/runtime/node/20/$debian_flavor.Dockerfile -t node20_$debian_flavor --build-arg NODE20_VERSION=$node20Version --build-arg BASE_IMAGE="docker.io/library/oryx_node_run_base_$debian_flavor" --build-arg NPM_VERSION=$NPM_VERSION --build-arg PM2_VERSION=$PM2_VERSION --build-arg NODE_APP_INSIGHTS_SDK_VERSION=$NODE_APP_INSIGHTS_SDK_VERSION --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
                rm -f ./nodejs-$debian_flavor-$node20Version.tar.gz
            ;;

            "22")
                curl -SL --output "nodejs-$debian_flavor-$node22Version.tar.gz" "https://oryxsdksdev.blob.core.windows.net/nodejs/nodejs-$debian_flavor-$node22Version.tar.gz"
                docker build -f ./images/runtime/node/22/$debian_flavor.Dockerfile -t node22_$debian_flavor --build-arg NODE22_VERSION=$node22Version --build-arg BASE_IMAGE="docker.io/library/oryx_node_run_base_$debian_flavor" --build-arg NPM_VERSION=$NPM_VERSION --build-arg PM2_VERSION=$PM2_VERSION --build-arg NODE_APP_INSIGHTS_SDK_VERSION=$NODE_APP_INSIGHTS_SDK_VERSION --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
                rm -f ./nodejs-$debian_flavor-$node22Version.tar.gz
            ;;
        esac
    ;;

    "php-fpm")
        docker build -f ./images/runtime/commonbase/phpFpmRuntimeBase.Dockerfile -t oryx_php_fpm_run_base_$debian_flavor --build-arg BASE_IMAGE="docker.io/library/oryx_run_base_$debian_flavor" .
        case $stack_version in
            "8.1")
                docker build -f ./images/runtime/php-fpm/8.1/$debian_flavor.Dockerfile -t phpfpm81_image_$debian_flavor --build-arg PHP_VERSION=$php81Version --build-arg PHP_SHA256=$php81Version_SHA --build-arg BASE_IMAGE="docker.io/library/oryx_php_fpm_run_base_$debian_flavor" --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
            ;;

            "8.2")
                docker build -f ./images/runtime/php-fpm/8.2/$debian_flavor.Dockerfile -t phpfpm82_image_$debian_flavor --build-arg PHP_VERSION=$php82Version --build-arg PHP_SHA256=$php82Version_SHA --build-arg BASE_IMAGE="docker.io/library/oryx_php_fpm_run_base_$debian_flavor" --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
            ;;

            "8.3")
                docker build -f ./images/runtime/php-fpm/8.3/$debian_flavor.Dockerfile -t phpfpm83_image_$debian_flavor --build-arg PHP_VERSION=$php83Version --build-arg PHP_SHA256=$php83Version_SHA --build-arg BASE_IMAGE="docker.io/library/oryx_php_fpm_run_base_$debian_flavor" --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
            ;;

            "8.4")
                docker build -f ./images/runtime/php-fpm/8.4/$debian_flavor.Dockerfile -t phpfpm84_image_$debian_flavor --build-arg PHP_VERSION=$php84Version --build-arg PHP_SHA256=$php84Version_SHA --build-arg BASE_IMAGE="docker.io/library/oryx_php_fpm_run_base_$debian_flavor" --build-arg USER_DOTNET_AI_VERSION=$USER_DOTNET_AI_VERSION --build-arg AI_CONNECTION_STRING=$AI_CONNECTION_STRING .
            ;;
        esac
    ;;

    "python")
        case $stack_version in
            "3.8")
                curl -SL --output "python-$debian_flavor-$python38Version.tar.gz" "https://oryxsdksdev.blob.core.windows.net/python/python-$debian_flavor-$python38Version.tar.gz"
                docker build -f ./images/runtime/python/template.Dockerfile -t python38_image_$debian_flavor --build-arg PYTHON_FULL_VERSION=$python38Version --build-arg PYTHON_VERSION=3.8 --build-arg PYTHON_MAJOR_VERSION=3 --build-arg DEBIAN_FLAVOR=$debian_flavor --build-arg BASE_IMAGE="docker.io/library/oryx_run_base_$debian_flavor" --build-arg SDK_STORAGE_BASE_URL_VALUE=$SDK_STORAGE_BASE_URL_VALUE .
                rm -f ./python-$debian_flavor-$python38Version.tar.gz
            ;;

            "3.9")
                docker build -f ./images/runtime/python/template.Dockerfile -t python39_image_$debian_flavor --build-arg PYTHON_FULL_VERSION=$python39Version --build-arg PYTHON_VERSION=3.9 --build-arg PYTHON_MAJOR_VERSION=3 --build-arg DEBIAN_FLAVOR=$debian_flavor --build-arg BASE_IMAGE="docker.io/library/oryx_run_base_$debian_flavor" --build-arg SDK_STORAGE_BASE_URL_VALUE=$SDK_STORAGE_BASE_URL_VALUE .
            ;;

            "3.10")
                docker build -f ./images/runtime/python/template.Dockerfile -t python310_image_$debian_flavor --build-arg PYTHON_FULL_VERSION=$python310Version --build-arg PYTHON_VERSION=3.10 --build-arg PYTHON_MAJOR_VERSION=3 --build-arg DEBIAN_FLAVOR=$debian_flavor --build-arg BASE_IMAGE="docker.io/library/oryx_run_base_$debian_flavor" --build-arg SDK_STORAGE_BASE_URL_VALUE=$SDK_STORAGE_BASE_URL_VALUE .
            ;;

            "3.11")
                docker build -f ./images/runtime/python/template.Dockerfile -t python311_image_$debian_flavor --build-arg PYTHON_FULL_VERSION=$python311Version --build-arg PYTHON_VERSION=3.11 --build-arg PYTHON_MAJOR_VERSION=3 --build-arg DEBIAN_FLAVOR=$debian_flavor --build-arg BASE_IMAGE="docker.io/library/oryx_run_base_$debian_flavor" --build-arg SDK_STORAGE_BASE_URL_VALUE=$SDK_STORAGE_BASE_URL_VALUE .
            ;;

            "3.12")
                docker build -f ./images/runtime/python/template.Dockerfile -t python312_image_$debian_flavor --build-arg PYTHON_FULL_VERSION=$python312Version --build-arg PYTHON_VERSION=3.12 --build-arg PYTHON_MAJOR_VERSION=3 --build-arg DEBIAN_FLAVOR=$debian_flavor --build-arg BASE_IMAGE="docker.io/library/oryx_run_base_$debian_flavor" --build-arg SDK_STORAGE_BASE_URL_VALUE=$SDK_STORAGE_BASE_URL_VALUE .
            ;;

            "3.13")
                docker build -f ./images/runtime/python/template.Dockerfile -t python313_image_$debian_flavor --build-arg PYTHON_FULL_VERSION=$python313Version --build-arg PYTHON_VERSION=3.13 --build-arg PYTHON_MAJOR_VERSION=3 --build-arg DEBIAN_FLAVOR=$debian_flavor --build-arg BASE_IMAGE="docker.io/library/oryx_run_base_$debian_flavor" --build-arg SDK_STORAGE_BASE_URL_VALUE=$SDK_STORAGE_BASE_URL_VALUE .
            ;;
        esac
    ;;
esac


                


