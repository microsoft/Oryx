FROM oryxdevmcr.azurecr.io/public/oryx/build:lts-versions AS main

###
# Build run script generators (to be used by the `oryx run-script` command)
###
FROM golang:1.11-stretch as startupScriptGens

# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .

ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}

RUN ./build.sh dotnetcore /opt/startupcmdgen/dotnet
RUN ./build.sh node       /opt/startupcmdgen/nodejs
RUN ./build.sh php        /opt/startupcmdgen/php
RUN ./build.sh python     /opt/startupcmdgen/python
###
# End build run script generators
###

FROM main AS final
COPY --from=yarn-cache-base /usr/local/share/yarn-cache /usr/local/share/yarn-cache
COPY --from=startupScriptGens /opt/startupcmdgen/ /opt/startupcmdgen/
# https://github.com/docker-library/python/issues/147
RUN set -ex \
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
    && rm -rf /var/lib/apt/lists/* \
    && cd $imagesDir \ 
    && . $buildDir/__nodeVersions.sh \
    && ./installPlatform.sh nodejs 8.0.0 \
    && ./installPlatform.sh nodejs 8.1.4 \
    && ./installPlatform.sh nodejs 8.2.1 \
    && ./installPlatform.sh nodejs 8.8.1 \
    && ./installPlatform.sh nodejs 8.9.4 \
    && ./installPlatform.sh nodejs 8.11.4 \
    && ./installPlatform.sh nodejs 8.12.0 \
    && ./installPlatform.sh nodejs 8.15.1 \
    && ./installPlatform.sh nodejs 9.4.0 \
    && ./installPlatform.sh nodejs 10.1.0 \
    && ./installPlatform.sh nodejs 10.10.0 \
    && ./installPlatform.sh nodejs 10.14.2 \
    && ./installPlatform.sh nodejs 10.21.0 \
    && ./installPlatform.sh nodejs 12.18.2 \
    && $imagesDir/build/installNpm.sh \
    && ln -s 8.0.0 /opt/nodejs/8.0 \
    && ln -s 8.1.4 /opt/nodejs/8.1 \
    && ln -s 8.2.1 /opt/nodejs/8.2 \
    && ln -s 8.8.1 /opt/nodejs/8.8 \
    && ln -s 8.9.4 /opt/nodejs/8.9 \
    && ln -s 8.11.4 /opt/nodejs/8.11 \
    && ln -s 8.12.0 /opt/nodejs/8.12 \
    && ln -s 8.15.1 /opt/nodejs/8.15 \
    && ln -s 9.4.0 /opt/nodejs/9.4 \
    && ln -s 9.4 /opt/nodejs/9 \
    && ln -s 10.1.0 /opt/nodejs/10.1 \
    && ln -s 10.10.0 /opt/nodejs/10.10 \
    && ln -s 10.14.2 /opt/nodejs/10.14 \
    && ln -s 10.21.0 /opt/nodejs/10.21 \
    && ln -s 12.18.2 /opt/nodejs/12.18 \
    && . $buildDir/__nodeVersions.sh \
    && ln -s 2.15.9 /opt/npm/2.15 \
    && ln -s 2.15 /opt/npm/2 \
    && ln -s 3.9.5 /opt/npm/3.9 \
    && ln -s 3.10.10 /opt/npm/3.10 \
    && ln -s 3.10 /opt/npm/3 \
    && ln -s 5.0.3 /opt/npm/5.0 \
    && ln -s 5.3.0 /opt/npm/5.3 \
    && ln -s 5.4.2 /opt/npm/5.4 \
    && ln -s 5.6.0 /opt/npm/5.6 \
    && ln -s 5.6 /opt/npm/5 \
    # Install Python SDKs
    && PYTHONIOENCODING="UTF-8" \
    && cd $imagesDir \
    && . $buildDir/__pythonVersions.sh \
    && ./installPlatform.sh python $PYTHON27_VERSION \
    && ./installPlatform.sh python $PYTHON36_VERSION \
    && . $buildDir/__pythonVersions.sh && set -ex \
    && [ -d "/opt/python/$PYTHON27_VERSION" ] && echo /opt/python/$PYTHON27_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && [ -d "/opt/python/$PYTHON36_VERSION" ] && echo /opt/python/$PYTHON36_VERSION/lib >> /etc/ld.so.conf.d/python.conf \
    && ldconfig \
    && . $buildDir/__pythonVersions.sh && set -ex \
    && ln -sfn $PYTHON27_VERSION /opt/python/2.7 \
    && ln -sfn 2.7 /opt/python/2 \
    && ln -sfn $PYTHON36_VERSION /opt/python/3.6 \
    && $imagesDir/build/createFlattenedDotNetCoreInstall.sh \
    && cp $imagesDir/build/dotNetCoreAll.Readme.md /opt/dotnet/Readme.md \
    # Install PHP SDKs
    && cd $imagesDir \
    && . $buildDir/__phpVersions.sh \
    && ./installPlatform.sh php $PHP56_VERSION \
    && ./installPlatform.sh php $PHP70_VERSION \
    && ./installPlatform.sh php $PHP72_VERSION \
    && ./installPlatform.sh php $PHP74_VERSION \
    && ln -s /opt/php/5.6 /opt/php/5
