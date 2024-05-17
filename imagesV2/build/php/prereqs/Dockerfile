# DisableDockerDetector "Below image not yet supported in the Docker Hub mirror"
FROM buildpack-deps:stretch AS php-build-prereqs
COPY imagesV2/build/php/prereqs /php
COPY build/__phpVersions.sh /php/
COPY imagesV2/build/python/prereqs/build.sh /tmp/
COPY imagesV2/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
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
        . /php/installPrereqs.sh ; \
    fi
