FROM debian:stretch-slim
ARG ORYX_BUILDIMAGE_TYPE

COPY --from=buildscriptgenerator /opt/buildscriptgen/ /opt/buildscriptgen/

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
# .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu57 \
        liblttng-ust0 \
        libssl1.0.2 \
        libstdc++6 \
        zlib1g \
    && rm -rf /var/lib/apt/lists/* \
    && chmod a+x /opt/buildscriptgen/GenerateBuildScript \
    && mkdir -p /opt/oryx \
    && ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx
    
ENV PATH="$PATH:/opt/oryx" \
    ORYX_BUILDIMAGE_TYPE="cli"