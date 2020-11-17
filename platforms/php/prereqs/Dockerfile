ARG DEBIAN_FLAVOR
FROM buildpack-deps:${DEBIAN_FLAVOR} AS php-build-prereqs
COPY platforms/php/prereqs /php
COPY build/__phpVersions.sh /php/
COPY platforms/python/prereqs/build.sh /tmp/
COPY images/receiveGpgKeys.sh /tmp/receiveGpgKeys.sh
RUN chmod +x /tmp/*.sh
RUN chmod +x /php/*.sh && . /php/installPrereqs.sh
