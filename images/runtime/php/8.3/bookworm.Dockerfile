ARG BASE_IMAGE
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26-bookworm AS startupCmdGen
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV GIT_COMMIT=${GIT_COMMIT} BUILD_NUMBER=${BUILD_NUMBER} RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
RUN chmod +x build.sh && ./build.sh php /opt/startupcmdgen/startupcmdgen

FROM ${BASE_IMAGE}
ARG PHP_VERSION
ARG PHP_SHA256
ARG PHP_SOURCE_BASE_URL=""
ARG AI_CONNECTION_STRING
ENV PHP_VERSION=${PHP_VERSION} PHP_SHA256=${PHP_SHA256} PHP_SOURCE_BASE_URL=${PHP_SOURCE_BASE_URL} \
    PHP_INI_DIR=/usr/local/etc/php \
    PHP_CFLAGS="-fstack-protector-strong -fpic -fpie -O2 -D_LARGEFILE_SOURCE -D_FILE_OFFSET_BITS=64" \
    PHP_LDFLAGS="-Wl,-O1 -Wl,--hash-style=both -pie" \
    GPG_KEYS="1198C0117593497A5EC5C199286AF1F9897469DC AFD8691FDAEDF03BDF6E460563F15A9B715376CA C28D937575603EB4ABB725861C0779DC5C0A9DE4" \
    ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}
ENV PHP_CPPFLAGS=${PHP_CFLAGS}
COPY images/runtime/php-fpm/8.3/docker-php-source images/runtime/php-fpm/8.3/docker-php-ext-* /usr/local/bin/
COPY images/runtime/php/apache2-foreground images/runtime/php/docker-php-entrypoint /usr/local/bin/
COPY images/runtime/php/build-php-apache.sh /tmp/
RUN set -eux; \
    case "$PHP_VERSION" in 8.3.*) ;; *) echo "Expected PHP 8.3, got $PHP_VERSION" >&2; exit 1 ;; esac; \
    sed -i 's/\r$//' /usr/local/bin/docker-php-* /usr/local/bin/apache2-foreground /tmp/build-php-apache.sh; \
    chmod +x /usr/local/bin/docker-php-* /usr/local/bin/apache2-foreground; \
    bash /tmp/build-php-apache.sh; \
    rm /tmp/build-php-apache.sh
COPY images/runtime/php/install-php-extensions.sh /tmp/
RUN sed -i 's/\r$//' /tmp/install-php-extensions.sh \
    && bash /tmp/install-php-extensions.sh \
    && rm /tmp/install-php-extensions.sh
COPY images/runtime/php/configure-apache.sh /tmp/
RUN set -eux; \
    sed -i 's/\r$//' /tmp/configure-apache.sh; \
    bash /tmp/configure-apache.sh; \
    rm /tmp/configure-apache.sh; \
    rm -rf /tmp/oryx /var/lib/apt/lists/*
COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx
ENV APACHE_CONFDIR=/etc/apache2 APACHE_ENVVARS=/etc/apache2/envvars \
    APACHE_RUN_USER=www-data APACHE_DOCUMENT_ROOT=/home/site/wwwroot APACHE_PORT=8080 \
    PHP_ORIGIN=php PATH_CA_CERTIFICATE=/etc/ssl/certs/ca-certificate.crt \
    LANG=C.UTF-8 LANGUAGE=C.UTF-8 LC_ALL=C.UTF-8 CNB_STACK_ID=oryx.stacks.skeleton
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"
WORKDIR /var/www/html
ENTRYPOINT ["docker-php-entrypoint"]
STOPSIGNAL SIGWINCH
EXPOSE 8080
CMD ["apache2-foreground"]
