FROM buildpack-deps:stretch AS php-build-prereqs
COPY platforms/php/prereqs /php
COPY build/__phpVersions.sh /php/
COPY platforms/python/prereqs/build.sh /tmp/
COPY images/receivePgpKeys.sh /tmp/receivePgpKeys.sh
RUN chmod +x /php/*.sh && . /php/installPrereqs.sh
