ARG DEBIAN_FLAVOR
FROM buildpack-deps:${DEBIAN_FLAVOR} AS php-build-prereqs
COPY images/build/php/prereqs /php
COPY build/__phpVersions.sh /php/
COPY images/build/python/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/*.sh
RUN chmod +x /php/*.sh && . /php/installPrereqs.sh
