FROM buildpack-deps:stretch AS php-build-prereqs
COPY images/build/php/prereqs /php
COPY build/__phpVersions.sh /php/
RUN chmod +x /php/*.sh && . /php/installPrereqs.sh
