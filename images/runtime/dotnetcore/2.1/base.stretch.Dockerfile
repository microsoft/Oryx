# dotnet tools are currently available as part of SDK so we need to create them in an sdk image
# and copy them to our final runtime image
ARG DEBIAN_FLAVOR
FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS tools-install
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-sos

FROM oryx-run-base-stretch

RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        \
# .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu57 \
        icu-devtools \
        libssl1.0.2 \
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

# Install ASP.NET Core
ENV ASPNETCORE_VERSION 2.1.18

RUN curl -SL --output aspnetcore.tar.gz https://dotnetcli.blob.core.windows.net/dotnet/aspnetcore/Runtime/$ASPNETCORE_VERSION/aspnetcore-runtime-$ASPNETCORE_VERSION-linux-x64.tar.gz \
    && aspnetcore_sha512='83d58102ba9d9b9a6f4f19ea799fd20939ff013b3d2e3348e363f41f30c1e902995f59538f9b04d8b671b310598206736ce7dd0acde51ce3847beb3262293d60' \
    && echo "$aspnetcore_sha512  aspnetcore.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf aspnetcore.tar.gz -C /usr/share/dotnet \
    && rm aspnetcore.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet

RUN dotnet-sos install

RUN rm -rf /tmp/oryx

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libgdiplus \
    && rm -rf /var/lib/apt/lists/*