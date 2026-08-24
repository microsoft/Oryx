ARG BASE_IMAGE

FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26-bookworm AS startupCmdGen
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME} \
    GIT_COMMIT=${GIT_COMMIT} \
    BUILD_NUMBER=${BUILD_NUMBER}
RUN chmod +x build.sh && ./build.sh php /opt/startupcmdgen/startupcmdgen

FROM ${BASE_IMAGE}
ARG IMAGES_DIR=/tmp/oryx/images

RUN set -eux; \
	{ \
		echo 'Package: php*'; \
		echo 'Pin: release *'; \
		echo 'Pin-Priority: -1'; \
	} > /etc/apt/preferences.d/no-debian-php

ENV PHPIZE_DEPS="autoconf dpkg-dev file g++ gcc libc-dev make pkg-config re2c" \
    PHP_INI_DIR=/usr/local/etc/php

RUN set -eux; \
	mkdir -p "$PHP_INI_DIR/conf.d"; \
	[ ! -d /var/www/html ]; \
	mkdir -p /var/www/html; \
	chown www-data:www-data /var/www/html; \
	chmod 1777 /var/www/html

ENV PHP_CFLAGS="-fstack-protector-strong -fpic -fpie -O2 -D_LARGEFILE_SOURCE -D_FILE_OFFSET_BITS=64" \
    PHP_CPPFLAGS="-fstack-protector-strong -fpic -fpie -O2 -D_LARGEFILE_SOURCE -D_FILE_OFFSET_BITS=64" \
    PHP_LDFLAGS="-Wl,-O1 -pie" \
    GPG_KEYS="D95C03BC702BE9515344AE3374E44BC9067701A5 016895DE9A475111D537A6E69134FF30BC5A99B5 5CFF17B64DC1C244F5D0EAC3E43535E2EB19010E"

ARG PHP_VERSION
ARG PHP_SHA256
ENV PHP_VERSION=${PHP_VERSION} \
    PHP_SHA256=${PHP_SHA256}

COPY images/receiveGpgKeys.sh ${IMAGES_DIR}/receiveGpgKeys.sh
COPY images/runtime/php-fpm/8.6/docker-php-ext-* images/runtime/php-fpm/8.6/docker-php-entrypoint /usr/local/bin/
COPY images/runtime/php-fpm/8.6/docker-php-source /usr/local/bin/
COPY images/runtime/php-fpm/8.6/scripts/ /opt/oryx/php/

RUN sed -i 's/\r$//' /usr/local/bin/docker-php-* /opt/oryx/php/*.sh \
	&& chmod +x /usr/local/bin/docker-php-* /opt/oryx/php/*.sh

RUN /opt/oryx/php/install-php-runtime-dependencies.sh
RUN /opt/oryx/php/download-and-verify-php-source.sh
RUN /opt/oryx/php/build-php-fpm.sh
RUN docker-php-ext-enable sodium

ENTRYPOINT ["docker-php-entrypoint"]
WORKDIR /var/www/html

RUN /opt/oryx/php/configure-php-fpm.sh
RUN /opt/oryx/php/install-bundled-php-extensions.sh
RUN /opt/oryx/php/install-third-party-php-extensions.sh
RUN /opt/oryx/php/install-nginx.sh

COPY images/runtime/php-fpm/nginx_conf/default.conf /etc/nginx/sites-available/default
COPY images/runtime/php-fpm/nginx_conf/fastcgi.conf /etc/nginx/fastcgi.conf
COPY images/runtime/php-fpm/nginx_conf/proxy_params /etc/nginx/proxy_params
COPY images/runtime/php-fpm/nginx_conf/fastcgi-php.conf /etc/nginx/snippets/fastcgi-php.conf

RUN /opt/oryx/php/configure-nginx.sh
RUN /opt/oryx/php/configure-php-runtime-settings.sh

ENV PHP_ORIGIN=php-fpm \
    NGINX_RUN_USER=www-data \
    NGINX_DOCUMENT_ROOT=/home/site/wwwroot \
    NGINX_PORT=8080 \
    LANG=C.UTF-8 \
    LANGUAGE=C.UTF-8 \
    LC_ALL=C.UTF-8 \
    CNB_STACK_ID=oryx.stacks.skeleton

ARG AI_CONNECTION_STRING
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx

STOPSIGNAL SIGQUIT
EXPOSE 9000
CMD ["php-fpm"]
