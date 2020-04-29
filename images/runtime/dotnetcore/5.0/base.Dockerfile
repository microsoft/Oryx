# dotnet tools are currently available as part of SDK so we need to create them in an sdk image
# and copy them to our final runtime image
FROM mcr.microsoft.com/dotnet/core/sdk:5.0 AS tools-install
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-sos
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-trace
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-dump
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-counters
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-gcdump

FROM debian:buster-slim

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        \
# .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu63 \
        libssl1.1 \
        libstdc++6 \
        zlib1g \
        lldb \
        curl \
        file \
    && rm -rf /var/lib/apt/lists/*

# Configure web servers to bind to port 80 when present
ENV ASPNETCORE_URLS=http://+:80 \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=tools-install /dotnetcore-tools /opt/dotnetcore-tools
ENV PATH="/opt/dotnetcore-tools:${PATH}"

# Install .NET Core
ENV DOTNET_VERSION 5.0.0-preview.3.20214.6

RUN curl -SL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Runtime/$DOTNET_VERSION/dotnet-runtime-$DOTNET_VERSION-linux-x64.tar.gz \
    && dotnet_sha512='459a5aab9dea6d27e15a55f8fdc4e44e6ed05b20c6d1bd9aba4fbc3b9176ffe79d9ae57b7757401e7c4ed0a11e483c6729b8a25f5af8dd6346dbb0fd025d0f2a' \
    && echo "$dotnet_sha512 dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet
    
# Install ASP.NET Core
ENV ASPNETCORE_VERSION 5.0.0-preview.3.20215.14

RUN curl -SL --output aspnetcore.tar.gz https://dotnetcli.azureedge.net/dotnet/aspnetcore/Runtime/$ASPNETCORE_VERSION/aspnetcore-runtime-$ASPNETCORE_VERSION-linux-x64.tar.gz \
    && aspnetcore_sha512='1839e36b03c7c1da497f8ee6f2b484591513742a8aae897281a4be17f22d1d7ca5e9d877e4b90ab6100136564efc41c47287ef9fa071441f7d2e203816e0f1b7' \
    && echo "$aspnetcore_sha512  aspnetcore.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf aspnetcore.tar.gz -C /usr/share/dotnet ./shared/Microsoft.AspNetCore.App \
    && rm aspnetcore.tar.gz

RUN dotnet-sos install

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libgdiplus \
    && rm -rf /var/lib/apt/lists/*