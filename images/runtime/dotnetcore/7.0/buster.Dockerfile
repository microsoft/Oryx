# dotnet tools are currently available as part of SDK so we need to create them in an sdk image
# and copy them to our final runtime image
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS tools-install
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-sos
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-trace
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-dump
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-counters
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-gcdump
RUN dotnet tool install --tool-path /dotnetcore-tools dotnet-monitor --version 7.*


FROM dotnetcore:base as final

COPY --from=tools-install /dotnetcore-tools /opt/dotnetcore-tools

COPY bin.zip /tmp
RUN unzip -q -o /tmp/bin.zip -d /defaulthome/hostingstart \
    && rm /tmp/bin.zip
RUN echo 'debconf debconf/frontend select Noninteractive' | debconf-set-selections

# Install .NET Core
RUN set -ex \
# based on resolution on https://github.com/NuGet/Announcements/issues/49#issue-795386700
    && apt-get remove ca-certificates -y \
    && apt-get purge ca-certificates -y \
    && apt-get update \
    && apt-get install -f ca-certificates=20200601~deb10u2 -y --no-install-recommends \
    && . ${BUILD_DIR}/__dotNetCoreRunTimeVersions.sh \
    && curl -SL --insecure --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Runtime/$NET_CORE_APP_70/dotnet-runtime-$NET_CORE_APP_70-linux-x64.tar.gz \
    && echo "$NET_CORE_APP_70_SHA dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf dotnet.tar.gz -C /usr/share/dotnet \
    && rm dotnet.tar.gz \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    # Install ASP.NET Core
    && . ${BUILD_DIR}/__dotNetCoreRunTimeVersions.sh \
    && curl -SL --insecure --output aspnetcore.tar.gz https://dotnetcli.azureedge.net/dotnet/aspnetcore/Runtime/$ASPNET_CORE_APP_70/aspnetcore-runtime-$ASPNET_CORE_APP_70-linux-x64.tar.gz \
    && echo "$ASPNET_CORE_APP_70_SHA aspnetcore.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -zxf aspnetcore.tar.gz -C /usr/share/dotnet ./shared/Microsoft.AspNetCore.App \
    && rm aspnetcore.tar.gz \
    && dotnet-sos install \
    && rm -rf ${BUILD_DIR}