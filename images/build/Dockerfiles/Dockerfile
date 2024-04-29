FROM oryxdevmcr.azurecr.io/public/oryx/build:lts-versions-debian-stretch AS main

###
# Build run script generators (to be used by the `oryx run-script` command)
###
# DisableDockerDetector "Below image not yet supported in the Docker Hub mirror"
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.20-bullseye as startupScriptGens

# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV DEBIAN_FLAVOR="stretch"

RUN ./build.sh dotnetcore /opt/startupcmdgen/dotnet
RUN ./build.sh golang     /opt/startupcmdgen/golang
RUN ./build.sh hugo       /opt/startupcmdgen/hugo
RUN ./build.sh node       /opt/startupcmdgen/nodejs
RUN ./build.sh php        /opt/startupcmdgen/php
RUN ./build.sh python     /opt/startupcmdgen/python
RUN ./build.sh ruby       /opt/startupcmdgen/ruby

###
# End build run script generators
###

FROM main AS final
COPY --from=startupScriptGens /opt/startupcmdgen/ /opt/startupcmdgen/

# https://github.com/docker-library/python/issues/147
RUN --mount=type=secret,id=oryx_sdk_storage_account_access_token \
    set -e \
    && export ORYX_SDK_STORAGE_ACCOUNT_ACCESS_TOKEN_PATH="/run/secrets/oryx_sdk_storage_account_access_token" \
    && tmpDir="/opt/tmp" \
    && buildDir="$tmpDir/build" \
    && imagesDir="$tmpDir/images" \
    # Install .NET Core SDKS
    && nugetPacakgesDir="/var/nuget" \
    && mkdir -p $nugetPacakgesDir \
    && NUGET_PACKAGES="$nugetPacakgesDir" \
    && . $buildDir/__dotNetCoreSdkVersions.sh \
    && DOTNET_SDK_VER=$DOT_NET_CORE_22_SDK_VERSION $imagesDir/build/installDotNetCore.sh \
    && DOTNET_SDK_VER=$DOT_NET_CORE_30_SDK_VERSION $imagesDir/build/installDotNetCore.sh \
    && rm -rf /tmp/NuGetScratch \
    && find $nugetPacakgesDir -type d -exec chmod 777 {} \; \
    && cd /opt/dotnet \
    # A temporary solution for AppService to make it builds work
    && ln -s $DOT_NET_CORE_22_SDK_VERSION 2.2 \
    && cd $imagesDir \ 
    && . $buildDir/__nodeVersions.sh \
    && $imagesDir/installPlatform.sh nodejs $NODE8_VERSION \
    && $imagesDir/installPlatform.sh nodejs 8.0.0 \
    && $imagesDir/installPlatform.sh nodejs 8.1.4 \
    && $imagesDir/installPlatform.sh nodejs 8.2.1 \
    && $imagesDir/installPlatform.sh nodejs 8.8.1 \
    && $imagesDir/installPlatform.sh nodejs 8.9.4 \
    && $imagesDir/installPlatform.sh nodejs 8.11.4 \
    && $imagesDir/installPlatform.sh nodejs 8.12.0 \
    && $imagesDir/installPlatform.sh nodejs 8.15.1 \
    && $imagesDir/installPlatform.sh nodejs 9.4.0 \
    && $imagesDir/installPlatform.sh nodejs 10.1.0 \
    && $imagesDir/installPlatform.sh nodejs 10.10.0 \
    && $imagesDir/installPlatform.sh nodejs 10.14.2 \
    && $imagesDir/installPlatform.sh nodejs 10.21.0 \
    && $imagesDir/installPlatform.sh nodejs 12.18.2 \
    && cd /opt/nodejs \
    && ln -s $NODE8_VERSION 8 \
    && ln -s 8.0.0 8.0 \
    && ln -s 8.1.4 8.1 \
    && ln -s 8.2.1 8.2 \
    && ln -s 8.8.1 8.8 \
    && ln -s 8.9.4 8.9 \
    && ln -s 8.11.4 8.11 \
    && ln -s 8.12.0 8.12 \
    && ln -s 8.15.1 8.15 \
    && ln -s 9.4.0 9.4 \
    && ln -s 9.4 9 \
    && ln -s 10.1.0 10.1 \
    && ln -s 10.10.0 10.10 \
    && ln -s 10.14.2 10.14 \
    && ln -s 10.21.0 10.21 \
    && ln -s 12.18.2 12.18 \
    && yarnCacheFolder="/usr/local/share/yarn-cache" \
    && mkdir -p $yarnCacheFolder \
    && chmod 777 $yarnCacheFolder \
    # Install Python SDKs
    && PYTHONIOENCODING="UTF-8" \
    && . $buildDir/__pythonVersions.sh \
    && $imagesDir/installPlatform.sh python $PYTHON27_VERSION \
    && $imagesDir/installPlatform.sh python $PYTHON36_VERSION \
    && [ -d "/opt/python/$PYTHON27_VERSION" ] && echo /opt/python/$PYTHON27_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && [ -d "/opt/python/$PYTHON36_VERSION" ] && echo /opt/python/$PYTHON36_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && cd /opt/python \
    && ln -sfn $PYTHON27_VERSION 2.7 \
    && ln -sfn 2.7 2 \
    && ln -sfn $PYTHON36_VERSION 3.6 \
    && $imagesDir/build/createFlattenedDotNetCoreInstall.sh \
    && cp $imagesDir/build/dotNetCoreAll.Readme.md /opt/dotnet/Readme.md \
    # Install PHP SDKs
    && . $buildDir/__phpVersions.sh \
    && $imagesDir/installPlatform.sh php $PHP70_VERSION \
    && $imagesDir/installPlatform.sh php $PHP72_VERSION \
    && $imagesDir/installPlatform.sh php $PHP74_VERSION \
    && $imagesDir/installPlatform.sh php $PHP56_VERSION \
    && cd /opt/php \
    && ln -s 5.6 5 \
    && echo "full" > /opt/oryx/.imagetype \
    && echo "DEBIAN|${DEBIAN_FLAVOR}" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype

