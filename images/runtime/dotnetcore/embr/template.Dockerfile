ARG BASE_IMAGE
ARG DOTNET_SDK_VERSION

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS tools-install
ARG VSS_NUGET_URI_PREFIXES
ARG VSS_NUGET_EXTERNAL_FEED_ENDPOINTS

RUN wget -q https://aka.ms/install-artifacts-credprovider.sh -O /tmp/install-artifacts-credprovider.sh \
    && bash /tmp/install-artifacts-credprovider.sh \
    && rm /tmp/install-artifacts-credprovider.sh

RUN --mount=type=secret,id=vss_nuget_accesstoken,target=/run/secrets/vss_nuget_accesstoken \
    if [ -f /run/secrets/vss_nuget_accesstoken ]; then \
        export VSS_NUGET_ACCESSTOKEN="$(cat /run/secrets/vss_nuget_accesstoken)"; \
    fi \
    && dotnet tool install --tool-path /dotnetcore-tools dotnet-sos \
    && dotnet tool install --tool-path /dotnetcore-tools dotnet-trace \
    && dotnet tool install --tool-path /dotnetcore-tools dotnet-dump \
    && dotnet tool install --tool-path /dotnetcore-tools dotnet-counters \
    && dotnet tool install --tool-path /dotnetcore-tools dotnet-gcdump \
    && dotnet tool install --tool-path /dotnetcore-tools dotnet-monitor --version 10.0.0-rc.1.25460.1

FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26-bookworm AS startupCmdGen
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME} \
    GIT_COMMIT=${GIT_COMMIT} \
    BUILD_NUMBER=${BUILD_NUMBER} \
    PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"
RUN chmod +x build.sh && ./build.sh dotnetcore /opt/startupcmdgen/startupcmdgen

FROM ${BASE_IMAGE} AS appInsights
ARG USER_DOTNET_AI_VERSION
RUN curl --fail --show-error --location \
        "https://oryxsdksdev-h4a7b3fscnhggycc.b02.azurefd.net/appinsights-agent/DotNetCoreAgent.${USER_DOTNET_AI_VERSION}.zip" \
        --output /tmp/appinsights.zip \
    && apt-get update \
    && apt-get install -y --no-install-recommends unzip \
    && unzip /tmp/appinsights.zip -d /DotNetCoreAgent

FROM ${BASE_IMAGE}

RUN apt-get update \
    && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        libc6 \
        libgcc-s1 \
        libgssapi-krb5-2 \
        libicu78 \
        libssl3 \
        libstdc++6 \
        zlib1g \
        lldb-18 \
        curl \
        file \
        libgdiplus \
        tzdata \
        unzip \
    && apt-get upgrade --assume-yes \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:80 \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_VERSION="10.0" \
    ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true \
    PATH="/opt/dotnetcore-tools:${PATH}"

COPY --from=tools-install /dotnetcore-tools /opt/dotnetcore-tools
ARG NET_CORE_APP_100
ARG ASPNET_CORE_APP_100

RUN set -ex \
    && mkdir -p /tmp/dotnet-download /usr/share/dotnet \
    && cd /tmp/dotnet-download \
    && curl --fail --show-error --location \
        --remote-name "https://builds.dotnet.microsoft.com/dotnet/Runtime/${NET_CORE_APP_100}/dotnet-runtime-${NET_CORE_APP_100}-linux-x64.tar.gz" \
        --remote-name "https://builds.dotnet.microsoft.com/dotnet/Runtime/${NET_CORE_APP_100}/dotnet-runtime-${NET_CORE_APP_100}-linux-x64.tar.gz.sha512" \
    && sha512sum -c "dotnet-runtime-${NET_CORE_APP_100}-linux-x64.tar.gz.sha512" \
    && tar -zxf "dotnet-runtime-${NET_CORE_APP_100}-linux-x64.tar.gz" -C /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && curl --fail --show-error --location \
        --remote-name "https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/${ASPNET_CORE_APP_100}/aspnetcore-runtime-${ASPNET_CORE_APP_100}-linux-x64.tar.gz" \
        --remote-name "https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/${ASPNET_CORE_APP_100}/aspnetcore-runtime-${ASPNET_CORE_APP_100}-linux-x64.tar.gz.sha512" \
    && sha512sum -c "aspnetcore-runtime-${ASPNET_CORE_APP_100}-linux-x64.tar.gz.sha512" \
    && tar -zxf "aspnetcore-runtime-${ASPNET_CORE_APP_100}-linux-x64.tar.gz" -C /usr/share/dotnet ./shared/Microsoft.AspNetCore.App \
    && dotnet-sos install \
    && rm -rf /tmp/dotnet-download /tmp/oryx

ARG AI_CONNECTION_STRING
ARG USER_DOTNET_AI_VERSION
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING} \
    USER_DOTNET_AI_VERSION=${USER_DOTNET_AI_VERSION} \
    PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt" \
    LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8" \
    CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

COPY --from=appInsights /DotNetCoreAgent /DotNetCoreAgent
COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx
