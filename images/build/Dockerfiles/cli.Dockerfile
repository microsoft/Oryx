ARG DEBIAN_FLAVOR
FROM debian:${DEBIAN_FLAVOR}-slim

ARG DEBIAN_FLAVOR

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
# .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libstdc++6 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/*

RUN echo "debian flavor is: ${DEBIAN_FLAVOR}" ;\
    if [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu63 \
            libssl1.1 \
        && rm -rf /var/lib/apt/lists/* ; \
    else \
        apt-get update \
        && apt-get install -y --no-install-recommends \
            libicu57 \
            liblttng-ust0 \
            libssl1.0.2 \
        && rm -rf /var/lib/apt/lists/* ; \
    fi

COPY --from=buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript
RUN mkdir -p /opt/oryx \
    && ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx
ENV PATH="$PATH:/opt/oryx"