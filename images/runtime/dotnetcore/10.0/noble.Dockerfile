# dotnet tools are currently available as part of SDK so we need to create them in an sdk image
# and copy them to our final runtime image
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS tools-install
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-sos
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-trace
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-dump
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-counters
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-gcdump
RUN DOTNET_ROLL_FORWARD=LatestMajor dotnet tool install --tool-path /dotnetcore-tools dotnet-monitor

# Startup script generator
FROM mcr.microsoft.com/mirror/docker/library/ubuntu:noble AS startupCmdGen

ARG GO_VERSION=1.25.1
ARG GO_SHA256="7716a0d940a0f6ae8e1f3b3f4f36299dc53e31b16840dbd171254312c41ca12e"
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified

RUN apt-get update \
 && apt-get install -y --no-install-recommends curl ca-certificates git build-essential \
 && curl -fsSL https://go.dev/dl/go${GO_VERSION}.linux-amd64.tar.gz -o go.tgz \
 && echo "${GO_SHA256}  go.tgz" | sha256sum -c - \
 && tar -C /usr/local -xzf go.tgz \
 && rm go.tgz

ENV GOPATH=/go \
    PATH=/go/bin:/usr/local/go/bin:$PATH \
    GIT_COMMIT=${GIT_COMMIT} \
    BUILD_NUMBER=${BUILD_NUMBER} \
    RELEASE_TAG_NAME=${RELEASE_TAG_NAME} \
    PATH_CA_CERTIFICATE=/etc/ssl/certs/ca-certificate.crt

RUN mkdir -p "$GOPATH/src" "$GOPATH/bin" && chmod -R 1777 "$GOPATH"
    
WORKDIR /go/src
COPY src/startupscriptgenerator/src .

RUN chmod +x build.sh && ./build.sh dotnetcore /opt/startupcmdgen/startupcmdgen
RUN apt-get purge -y build-essential git curl && apt-get autoremove -y && rm -rf /var/lib/apt/lists/*


FROM mcr.microsoft.com/mirror/docker/library/ubuntu:noble
ARG BUILD_DIR=/tmp/oryx/build
ADD build ${BUILD_DIR}

ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        # .NET Core dependencies
        libc6 \
        libgcc-s1 \
        libgssapi-krb5-2 \
        libicu74 \
        libssl3 \
        libstdc++6 \
        zlib1g \
        lldb-18 \
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

ARG NET_CORE_APP_100
ARG ASPNET_CORE_APP_100

# Install .NET Core
RUN set -ex \
# based on resolution on https://github.com/NuGet/Announcements/issues/49#issue-795386700
    && apt-get remove ca-certificates -y \
    && apt-get purge ca-certificates -y \
    && apt-get update \
    && apt-get install -f ca-certificates -y --no-install-recommends \
    && curl --fail --show-error --location \
        --remote-name https://builds.dotnet.microsoft.com/dotnet/Runtime/$NET_CORE_APP_100/dotnet-runtime-$NET_CORE_APP_100-linux-x64.tar.gz \
        --remote-name https://builds.dotnet.microsoft.com/dotnet/Runtime/$NET_CORE_APP_100/dotnet-runtime-$NET_CORE_APP_100-linux-x64.tar.gz.sha512 \
    && sha512sum -c dotnet-runtime-$NET_CORE_APP_100-linux-x64.tar.gz.sha512 \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet-runtime-$NET_CORE_APP_100-linux-x64.tar.gz -C /usr/share/dotnet \
    && rm dotnet-runtime-$NET_CORE_APP_100-linux-x64.tar.gz \
    && rm dotnet-runtime-$NET_CORE_APP_100-linux-x64.tar.gz.sha512 \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    # Install ASP.NET Core
    && curl --fail --show-error --location \
        --remote-name https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/$ASPNET_CORE_APP_100/aspnetcore-runtime-$ASPNET_CORE_APP_100-linux-x64.tar.gz \
        --remote-name https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/$ASPNET_CORE_APP_100/aspnetcore-runtime-$ASPNET_CORE_APP_100-linux-x64.tar.gz.sha512 \
    && sha512sum -c aspnetcore-runtime-$ASPNET_CORE_APP_100-linux-x64.tar.gz.sha512 \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf aspnetcore-runtime-$ASPNET_CORE_APP_100-linux-x64.tar.gz -C /usr/share/dotnet ./shared/Microsoft.AspNetCore.App \
    && rm aspnetcore-runtime-$ASPNET_CORE_APP_100-linux-x64.tar.gz \
    && rm aspnetcore-runtime-$ASPNET_CORE_APP_100-linux-x64.tar.gz.sha512 \
    && dotnet-sos install \
    && rm -rf ${BUILD_DIR}

# Bake Application Insights key from pipeline variable into final image
ARG AI_CONNECTION_STRING
ARG USER_DOTNET_AI_VERSION
ENV USER_DOTNET_AI_VERSION=${USER_DOTNET_AI_VERSION}
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING} 
ENV DOTNET_VERSION="10.0"
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