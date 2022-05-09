ARG DEBIAN_FLAVOR
FROM buildpack-deps:${DEBIAN_FLAVOR}
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR

COPY --from=buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/
COPY --from=support-files-image-for-build /tmp/oryx/ /opt/tmp

RUN if [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu63 \
            libcurl4 \
            libssl1.1 \
        && rm -rf /var/lib/apt/lists/* ; \
    else \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libcurl3 \
            libicu57 \
            liblttng-ust0 \
            libssl1.0.2 \
        && rm -rf /var/lib/apt/lists/* ; \
    fi

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
# .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libstdc++6 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/* \
    && chmod a+x /opt/buildscriptgen/GenerateBuildScript \
    && mkdir -p /opt/oryx \
    && ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx \
    && echo "cli" > /opt/oryx/.imagetype

RUN tmpDir="/opt/tmp" \
    && cp -f $tmpDir/images/build/benv.sh /opt/oryx/benv \
    && chmod +x /opt/oryx/benv

ENV ORYX_SDK_STORAGE_BASE_URL="https://oryx-cdn.microsoft.io"
ENV ENABLE_DYNAMIC_INSTALL="true"
ENV PATH="$PATH:/opt/oryx"
ENV DYNAMIC_INSTALL_ROOT_DIR="/opt"

ENTRYPOINT [ "benv" ]