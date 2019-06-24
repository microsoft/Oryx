FROM buildpack-deps:stable AS php-build-prereqs
COPY images/build/php/prereqs /php
COPY build/__php-versions.sh /php/
RUN chmod +x /php/*.sh && . /php/installPrereqs.sh
