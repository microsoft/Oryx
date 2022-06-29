# dotnet tools are currently available as part of SDK so we need to create them in an sdk image
# and copy them to our final runtime image
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS tools-install
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-sos --version 5.0.236902
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-trace --version 5.0.236902
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-dump --version 5.0.236902
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-counters --version 5.0.236902
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-gcdump --version 5.0.236902
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-monitor --version 6.1.*

FROM mcr.microsoft.com/mirror/docker/library/debian:bullseye-slim
ARG BUILD_DIR=/tmp/oryx/build
ADD build ${BUILD_DIR}

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        \
        # .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu67 \
        libssl1.1 \
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

# Install .NET Core
RUN set -ex \
    && . ${BUILD_DIR}/__dotNetCoreRunTimeVersions.sh \
    && curl -SL --output dotnet.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$NET_CORE_APP_31/dotnet-runtime-$NET_CORE_APP_31-linux-x64.tar.gz \
    && echo "$NET_CORE_APP_31_SHA dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    # Install ASP.NET Core
    && . ${BUILD_DIR}/__dotNetCoreRunTimeVersions.sh \
    && curl -SL --output aspnetcore.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/aspnetcore/Runtime/$ASPNET_CORE_APP_31/aspnetcore-runtime-$ASPNET_CORE_APP_31-linux-x64.tar.gz \
    && echo "$ASPNET_CORE_APP_31_SHA aspnetcore.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf aspnetcore.tar.gz -C /usr/share/dotnet ./shared/Microsoft.AspNetCore.App \
    && rm aspnetcore.tar.gz \
    && dotnet-sos install \
    && rm -rf ${BUILD_DIR}
