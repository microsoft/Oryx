ARG DEBIAN_FLAVOR
FROM buildpack-deps:${DEBIAN_FLAVOR} AS php-build-prereqs
ARG DEBIAN_FLAVOR
COPY images/build/php/prereqs /php
COPY build/__phpVersions.sh /php/
COPY images/build/python/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/*.sh
#RUN chmod +x /php/*.sh && . /php/installPrereqs.sh
RUN if [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
    apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
        libsodium-dev \
    --no-install-recommends && rm -r /var/lib/apt/lists/* ; \
    else \
        chmod +x /php/*.sh && . /php/installPrereqs.sh ; \
    fi 
