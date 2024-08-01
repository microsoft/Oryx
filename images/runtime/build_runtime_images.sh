#!/bin/bash

stack_name = $1;
stack_version = $2;
debian_flavor = $3;

set -ex

declare -r REPO_DIR=$( cd .. && cd .. && cd .. && cd .. && pwd )

echo "Current directory: $(pwd)"

source $REPO_DIR/build/defaultVersions.sh

case stack_name in
    "dotnet") 
       case stack_version in
            "6.0")
                docker build -f ./dotnet/6.0/$debian_flavor.Dockerfile -t dotnet6_image -build-arg NET_CORE_APP_60_SHA=$(NET_CORE_APP_60_SHA) --build-arg ASPNET_CORE_APP_60_SHA=$(ASPNET_CORE_APP_60_SHA) --build-arg NET_CORE_APP_60=$(NET_CORE_APP_60) --build-arg ASPNET_CORE_APP_60=$(ASPNET_CORE_APP_60)
            ;;

            "7.0")
                docker build -f ./dotnet/7.0/$debian_flavor.Dockerfile -t dotnet7_image -build-arg NET_CORE_APP_70_SHA=$(NET_CORE_APP_70_SHA) --build-arg ASPNET_CORE_APP_70_SHA=$(ASPNET_CORE_APP_70_SHA) --build-arg NET_CORE_APP_70=$(NET_CORE_APP_70) --build-arg ASPNET_CORE_APP_70=$(ASPNET_CORE_APP_70)
            ;;

            8.0)
                docker build -f ./dotnet/8.0/$debian_flavor.Dockerfile -t dotnet8_image -build-arg NET_CORE_APP_80_SHA=$(NET_CORE_APP_80_SHA) --build-arg ASPNET_CORE_APP_80_SHA=$(ASPNET_CORE_APP_80_SHA) --build-arg NET_CORE_APP_80=$(NET_CORE_APP_80) --build-arg ASPNET_CORE_APP_80=$(ASPNET_CORE_APP_80)
            ;;

            9.0)
                docker build -f ./dotnet/9.0/$debian_flavor.Dockerfile -t dotnet9_image -build-arg NET_CORE_APP_90_SHA=$(NET_CORE_APP_90_SHA) --build-arg ASPNET_CORE_APP_90_SHA=$(ASPNET_CORE_APP_90_SHA) --build-arg NET_CORE_APP_90=$(NET_CORE_APP_90) --build-arg ASPNET_CORE_APP_90=$(ASPNET_CORE_APP_90)
            ;;
    ;;


    "node")
        case stack_version in
            18)
                docker build -f ./node/18/$debian_flavor.Dockerfile -t dotnet6_image -build-arg NET_CORE_APP_60_SHA=$(NET_CORE_APP_60_SHA) --build-arg ASPNET_CORE_APP_60_SHA=$(ASPNET_CORE_APP_60_SHA) --build-arg NET_CORE_APP_60=$(NET_CORE_APP_60) --build-arg ASPNET_CORE_APP_60=$(ASPNET_CORE_APP_60)
            ;;

            20)

            ;;
                


