ARG DEBIAN_FLAVOR
FROM buildpack-deps:${DEBIAN_FLAVOR} AS php-buildpack-prereqs
ARG DEBIAN_FLAVOR
ENV DEBIAN_FLAVOR=$DEBIAN_FLAVOR
COPY platforms/php/prereqs /php
COPY build/__phpVersions.sh /php/
COPY platforms/php/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh

RUN if [ "${DEBIAN_FLAVOR}" = "stretch" ]; then \
        sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch-updates/# deb http:\/\/deb.debian.org\/debian stretch-updates/g' /etc/apt/sources.list  \
        && sed -i 's/^deb http:\/\/security.debian.org\/debian-security stretch/deb http:\/\/archive.debian.org\/debian-security stretch/g' /etc/apt/sources.list \
        && sed -i 's/^deb http:\/\/deb.debian.org\/debian stretch/deb http:\/\/archive.debian.org\/debian stretch/g' /etc/apt/sources.list ; \
    fi

RUN chmod +x /tmp/*.sh && chmod +x /php/*.sh
RUN if [ "${DEBIAN_FLAVOR}" = "buster" ]; then \
    apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y \
        $PHPIZE_DEPS \
        ca-certificates \
        curl \
        xz-utils \
        libsodium-dev \
        libncurses5 \
    --no-install-recommends && rm -r /var/lib/apt/lists/* ; \
    else \
        export DEBIAN_FLAVOR=${DEBIAN_FLAVOR} ; \
        . /php/installPrereqs.sh ; \
    fi
