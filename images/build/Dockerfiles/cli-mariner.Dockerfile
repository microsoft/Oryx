# Use the mariner base image
FROM mcr.microsoft.com/cbl-mariner/base/core:2.0 as main
ARG SDK_STORAGE_BASE_URL_VALUE="https://oryx-cdn.microsoft.io"
ARG AI_CONNECTION_STRING
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR

COPY --from=oryxdevmcr.azurecr.io/private/oryx/buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/
COPY --from=oryxdevmcr.azurecr.io/private/oryx/support-files-image-for-build /tmp/oryx/ /opt/tmp

ENV ORYX_SDK_STORAGE_BASE_URL=${SDK_STORAGE_BASE_URL_VALUE} \
    ENABLE_DYNAMIC_INSTALL="true" \
    PATH="/usr/local/go/bin:/opt/python/latest/bin:/opt/oryx:/opt/yarn/stable/bin:/opt/hugo/lts:$PATH" \
    DYNAMIC_INSTALL_ROOT_DIR="/opt" \
    PYTHONIOENCODING="UTF-8" \
    LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8" \
    ORYX_AI_CONNECTION_STRING="${AI_CONNECTION_STRING}" \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1"

# Install an assortment of traditional tooling (unicode, SSL, HTTP, etc.)
# Also install some PHP requirements (unsure why it's gated by buster flavor, maybe due to some libraries being released per debian flavor, but we should try to install these pre-reqs no matter what)
RUN tdnf update -y \
    && tdnf install -y \
       nodejs \ 
       python \ 
       dotnet-sdk-7.0 \
       golang \ 
       php \
       ruby \
       java \
       maven 
  
RUN tdnf update -y \
    && tdnf install -y \
# .NET Core dependencies for running Oryx
        rsync \
        libgdiplus \
         # Required for mysqlclient
        mysql \
    && rm -rf /var/lib/apt/lists/* \
    && chmod a+x /opt/buildscriptgen/GenerateBuildScript \
    && mkdir -p /opt/oryx \
    && ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx \
    && echo "cli" > /opt/oryx/.imagetype \
    && echo "MARINER" | tr '[a-z]' '[A-Z]' > /opt/oryx/.ostype

RUN tmpDir="/opt/tmp" \
    && cp -f $tmpDir/images/build/benv.sh /opt/oryx/benv \
    && cp -f $tmpDir/images/build/logger.sh /opt/oryx/logger \
    && chmod +x /opt/oryx/benv \
    && chmod +x /opt/oryx/logger

ENTRYPOINT [ "benv" ]