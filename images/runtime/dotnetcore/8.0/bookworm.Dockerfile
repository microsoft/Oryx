# dotnet tools are currently available as part of SDK so we need to create them in an sdk image
# and copy them to our final runtime image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS tools-install
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-sos
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-trace
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-dump
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-counters
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-gcdump
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-monitor --version 8.*

# Startup script generator
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.20-bookworm as startupCmdGen

# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
# Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"
RUN chmod +x build.sh && ./build.sh dotnetcore /opt/startupcmdgen/startupcmdgen


FROM mcr.microsoft.com/mirror/docker/library/debian:bookworm-slim
ARG BUILD_DIR=/tmp/oryx/build
ADD build ${BUILD_DIR}

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        # .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu72 \
        libssl3 \
        libstdc++6 \
        zlib1g \
        lldb \
        curl \
        file \
        libgdiplus \
    && apt-get upgrade --assume-yes \
    && rm -rf /var/lib/apt/lists/*

# Configure web servers to bind to port 80 when present
ENV ASPNETCORE_URLS=http://+:80 \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true \
    PATH="/opt/dotnetcore-tools:${PATH}"

COPY --from=tools-install /dotnetcore-tools /opt/dotnetcore-tools

RUN . ${BUILD_DIR}/defaultVersions.sh

ARG NET_CORE_APP_80_SHA=$NET_CORE_APP_80_SHA
ARG ASPNET_CORE_APP_80_SHA=$ASPNET_CORE_APP_80_SHA
ARG NET_CORE_APP_80=$NET_CORE_APP_80
ARG ASPNET_CORE_APP_80=$ASPNET_CORE_APP_80

# Install .NET Core
RUN set -ex \
# based on resolution on https://github.com/NuGet/Announcements/issues/49#issue-795386700
    && apt-get remove ca-certificates -y \
    && apt-get purge ca-certificates -y \
    && apt-get update \
    && apt-get install -f ca-certificates -y --no-install-recommends \
    # && . ${BUILD_DIR}/__dotNetCoreRunTimeVersions.sh \
    && curl -SL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Runtime/$NET_CORE_APP_80/dotnet-runtime-$NET_CORE_APP_80-linux-x64.tar.gz \
    && echo "$NET_CORE_APP_80_SHA dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    # Install ASP.NET Core
    # && . ${BUILD_DIR}/__dotNetCoreRunTimeVersions.sh \
    && curl -SL --output aspnetcore.tar.gz https://dotnetcli.azureedge.net/dotnet/aspnetcore/Runtime/$ASPNET_CORE_APP_80/aspnetcore-runtime-$ASPNET_CORE_APP_80-linux-x64.tar.gz \
    && echo "$ASPNET_CORE_APP_80_SHA aspnetcore.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf aspnetcore.tar.gz -C /usr/share/dotnet ./shared/Microsoft.AspNetCore.App \
    && rm aspnetcore.tar.gz \
    && dotnet-sos install \
    && rm -rf ${BUILD_DIR}

# Bake Application Insights key from pipeline variable into final image
ARG AI_CONNECTION_STRING
ARG USER_DOTNET_AI_VERSION
ENV USER_DOTNET_AI_VERSION=${USER_DOTNET_AI_VERSION}
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING} 
ENV DOTNET_VERSION="8.0"
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true
#Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

# Oryx++ Builder variables
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
COPY DotNetCoreAgent.${USER_DOTNET_AI_VERSION}.zip /DotNetCoreAgent/appinsights.zip
RUN set -e \
    && echo $USER_DOTNET_AI_VERSION \ 
    && ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && apt-get update \
    && apt-get install unzip -y \ 
    && apt-get upgrade --assume-yes \
    && cd DotNetCoreAgent \
    && unzip appinsights.zip && rm appinsights.zip