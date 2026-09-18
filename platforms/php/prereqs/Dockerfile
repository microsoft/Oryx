ARG OS_FLAVOR
FROM mcr.microsoft.com/mirror/docker/library/buildpack-deps:${OS_FLAVOR} AS php-buildpack-prereqs
ARG OS_FLAVOR
ENV OS_FLAVOR=$OS_FLAVOR

COPY images/runtime/scripts/configure-debian-bullseye-snapshot.sh /usr/local/bin/
RUN if [ "${OS_FLAVOR}" = "bullseye" ]; then \
        chmod +x /usr/local/bin/configure-debian-bullseye-snapshot.sh; \
        /usr/local/bin/configure-debian-bullseye-snapshot.sh; \
    fi \
    && rm /usr/local/bin/configure-debian-bullseye-snapshot.sh

COPY platforms/php/prereqs /php
COPY platforms/php/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh

RUN chmod +x /tmp/*.sh && chmod +x /php/*.sh
RUN export OS_FLAVOR=${OS_FLAVOR} && . /php/installPrereqs.sh
