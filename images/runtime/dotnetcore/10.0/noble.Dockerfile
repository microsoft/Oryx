# syntax=docker/dockerfile:1.2
ARG INCLUDE_AZURELINUX_CERTS=true
ARG VSS_NUGET_URI_PREFIXES
ARG VSS_NUGET_EXTERNAL_FEED_ENDPOINTS

# dotnet tools are currently available as part of SDK so we need to create them in an sdk image
# and copy them to our final runtime image
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS tools-install
ARG VSS_NUGET_URI_PREFIXES
ARG VSS_NUGET_EXTERNAL_FEED_ENDPOINTS

# Download the artifact credential provider
RUN wget -qO- https://aka.ms/install-artifacts-credprovider.sh | bash

# Install dotnet tools with authentication
RUN --mount=type=secret,id=vss_nuget_accesstoken,target=/run/secrets/vss_nuget_accesstoken \
    VSS_NUGET_ACCESSTOKEN=$(cat /run/secrets/vss_nuget_accesstoken) \
    dotnet tool install --tool-path /dotnetcore-tools dotnet-sos && \
    dotnet tool install --tool-path /dotnetcore-tools dotnet-trace && \
    dotnet tool install --tool-path /dotnetcore-tools dotnet-dump && \
    dotnet tool install --tool-path /dotnetcore-tools dotnet-counters && \
    dotnet tool install --tool-path /dotnetcore-tools dotnet-gcdump && \
    dotnet tool install --tool-path /dotnetcore-tools dotnet-monitor --version 10.0.0-rc.1.25460.1

# Startup script generator
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.25.3-bookworm AS startupCmdGen

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

# Stage: (optional) pull Azure Linux CA certificates so we can merge them into BaseOS trust store
# Debian images (especially slim variants) can lag in ca-certificates updates; we optionally add Azure Linux certs.
FROM mcr.microsoft.com/azurelinux/base/core:3.0 AS azurelinux-core
ARG INCLUDE_AZURELINUX_CERTS
RUN if [ "$(printf '%s' "$INCLUDE_AZURELINUX_CERTS" | tr '[:upper:]' '[:lower:]')" = "true" ]; then \
		set -eux; \
		tdnf makecache; \
		tdnf install -y ca-certificates; \
		update-ca-trust extract; \
		tdnf clean all; \
	fi

FROM mcr.microsoft.com/mirror/docker/library/ubuntu:noble
ARG BUILD_DIR=/tmp/oryx/build
ARG INCLUDE_AZURELINUX_CERTS
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

# Copy Azure linux certs to a temporary location
RUN mkdir -p /tmp/azurelinux-ca-certs && \
	chmod 755 /tmp/azurelinux-ca-certs;
COPY --from=azurelinux-core /etc/pki/ca-trust/extracted/pem/tls-ca-bundle.pem /tmp/azurelinux-ca-certs/tls-ca-bundle.pem

COPY images/runtime/scripts/install-azurelinux-certs.sh /usr/local/bin/install-azurelinux-certs.sh

# Add Azure Linux certs to Debian's CA store if the flag is set to true
RUN set -e; \
    if [ "$(printf '%s' "$INCLUDE_AZURELINUX_CERTS" | tr '[:upper:]' '[:lower:]')" = "true" ]; then \
        chmod +x /usr/local/bin/install-azurelinux-certs.sh; \
        /usr/local/bin/install-azurelinux-certs.sh /tmp/azurelinux-ca-certs /tmp/azurelinux-ca-certs/tls-ca-bundle.pem; \
    fi;

# Cleanup script and temporary files
RUN	rm -f /usr/local/bin/install-azurelinux-certs.sh; \
	rm -rf /tmp/azurelinux-ca-certs;

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