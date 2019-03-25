FROM buildpack-deps:stable AS php-build-prereqs
COPY images/build/php/prereqs /php
COPY build/__php-versions.sh /php/
RUN set -eux && chmod +x /php/*.sh && /php/install-prereqs.sh
