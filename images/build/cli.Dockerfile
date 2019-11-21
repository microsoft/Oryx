FROM oryxdevmcr.azurecr.io/public/oryx/build:slim as build

FROM debian:stretch-slim

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
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /opt/oryx /opt/oryx
COPY --from=build /opt/buildscriptgen/ /opt/buildscriptgen/
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript
ENV PATH="$PATH:/opt/oryx"