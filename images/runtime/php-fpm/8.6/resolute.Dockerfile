ARG BASE_IMAGE

FROM mcr.microsoft.com/oss/go/microsoft/golang:1.26-bookworm AS startupcmdgen
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
RUN chmod +x build.sh && ./build.sh php /opt/startupcmdgen/startupcmdgen

FROM ${BASE_IMAGE} AS phpbuilder

ENV PHPIZE_DEPS="autoconf dpkg-dev file g++ gcc libc-dev make pkg-config re2c"
ENV PHP_INI_DIR=/usr/local/etc/php
ENV PHP_CFLAGS="-fstack-protector-strong -fpic -fpie -O2 -D_LARGEFILE_SOURCE -D_FILE_OFFSET_BITS=64"
ENV PHP_CPPFLAGS="${PHP_CFLAGS}"
ENV PHP_LDFLAGS="-Wl,-O1 -pie"

ARG PHP_VERSION
ARG PHP_SHA256
ENV PHP_VERSION=${PHP_VERSION}
ENV PHP_SHA256=${PHP_SHA256}
ENV PHP_URL="https://downloads.php.net/~svpernova09/php-8.6.0beta1.tar.xz"
ENV PHP_ASC_URL="https://downloads.php.net/~svpernova09/php-8.6.0beta1.tar.xz.asc"
ENV GPG_KEYS="D95C03BC702BE9515344AE3374E44BC9067701A5 016895DE9A475111D537A6E69134FF30BC5A99B5 5CFF17B64DC1C244F5D0EAC3E43535E2EB19010E"

ARG XDEBUG_SOURCE_COMMIT=db5e99bf8109ebf6307268fe1ff844001ed47998
ARG XDEBUG_SOURCE_SHA256=f765b9d557a2d32ff59074e67cde51e0ef7f11f4ba5460e412d951f31cff9a8e
ARG REDIS_SOURCE_COMMIT=777f7377674a8f5d23a7ba739d165daeebc13e47
ARG REDIS_SOURCE_SHA256=59cfe640de5d40770ff30f028d8aa298a11410194dd085e347e06607a8a91e70

RUN set -eux; \
    test "$PHP_VERSION" = "8.6.0beta1"; \
    test "$PHP_SHA256" = "e44571d75b368b36a2d6db6878705e5512af5d2a78e31463c49796a94018d282"; \
    { \
        echo 'Package: php*'; \
        echo 'Pin: release *'; \
        echo 'Pin-Priority: -1'; \
    } > /etc/apt/preferences.d/no-ubuntu-php; \
    apt-get update; \
    DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        $PHPIZE_DEPS \
        bison \
        libargon2-dev \
        libbz2-dev \
        libcurl4-openssl-dev \
        libedit-dev \
        libenchant-2-dev \
        libffi-dev \
        libfreetype-dev \
        libgmp-dev \
        libicu-dev \
        libjpeg-dev \
        libldap-dev \
        libonig-dev \
        libpng-dev \
        libpq-dev \
        libreadline-dev \
        libsodium-dev \
        libsnmp-dev \
        libsqlite3-dev \
        libssl-dev \
        libtidy-dev \
        libxml2-dev \
        libxslt1-dev \
        libzip-dev \
        unixodbc-dev \
        zlib1g-dev; \
    rm -rf /var/lib/apt/lists/*

COPY images/runtime/php-fpm/8.5/docker-php-source /usr/local/bin/docker-php-source
COPY images/runtime/php-fpm/8.5/docker-php-ext-configure /usr/local/bin/docker-php-ext-configure
COPY images/runtime/php-fpm/8.5/docker-php-ext-enable /usr/local/bin/docker-php-ext-enable
COPY images/runtime/php-fpm/8.5/docker-php-ext-install /usr/local/bin/docker-php-ext-install
RUN sed -i 's/\r$//' /usr/local/bin/docker-php-* \
    && chmod +x /usr/local/bin/docker-php-*

RUN set -eux; \
    mkdir -p /usr/src "$PHP_INI_DIR/conf.d" /var/www/html; \
    curl -fsSL --retry 5 --retry-delay 5 -o /usr/src/php.tar.xz "$PHP_URL"; \
    echo "$PHP_SHA256 */usr/src/php.tar.xz" | sha256sum -c -; \
    curl -fsSL --retry 5 --retry-delay 5 -o /usr/src/php.tar.xz.asc "$PHP_ASC_URL"; \
    export GNUPGHOME="$(mktemp -d)"; \
    for key in $GPG_KEYS; do \
        gpg --batch --keyserver keyserver.ubuntu.com --recv-keys "$key"; \
    done; \
    gpg --batch --verify /usr/src/php.tar.xz.asc /usr/src/php.tar.xz; \
    gpgconf --kill all; \
    rm -rf "$GNUPGHOME" /usr/src/php.tar.xz.asc; \
    docker-php-source extract; \
    cd /usr/src/php; \
    gnuArch="$(dpkg-architecture --query DEB_BUILD_GNU_TYPE)"; \
    debMultiarch="$(dpkg-architecture --query DEB_BUILD_MULTIARCH)"; \
    export CFLAGS="$PHP_CFLAGS" CPPFLAGS="$PHP_CPPFLAGS" LDFLAGS="$PHP_LDFLAGS"; \
    export PHP_BUILD_PROVIDER='https://github.com/microsoft/Oryx' PHP_UNAME='Linux - Oryx'; \
    ./configure \
        --build="$gnuArch" \
        --sysconfdir="${PHP_INI_DIR%/php}" \
        --with-config-file-path="$PHP_INI_DIR" \
        --with-config-file-scan-dir="$PHP_INI_DIR/conf.d" \
        --enable-option-checking=fatal \
        --with-mhash \
        --enable-pic \
        --enable-mysqlnd \
        --with-password-argon2 \
        --with-sodium=shared \
        --with-pdo-sqlite=/usr \
        --with-sqlite3=/usr \
        --with-curl \
        --with-iconv \
        --with-openssl \
        --with-readline \
        --with-zlib \
        --disable-phpdbg \
        --with-libdir="lib/$debMultiarch" \
        --disable-cgi \
        --enable-fpm \
        --with-fpm-user=www-data \
        --with-fpm-group=www-data; \
    make -j "$(nproc)"; \
    find -type f -name '*.a' -delete; \
    make install; \
    find /usr/local -type f -perm /0111 -exec sh -c 'strip --strip-all "$1" 2>/dev/null || true' _ '{}' +; \
    cp -v php.ini-* "$PHP_INI_DIR/"; \
    cd /; \
    docker-php-source delete; \
    php --version

RUN set -eux; \
    docker-php-ext-configure gd --with-freetype --with-jpeg; \
    docker-php-ext-configure pdo_odbc --with-pdo-odbc=unixODBC,/usr; \
    cd /usr/src/php/ext/odbc; \
    phpize; \
    sed -ri 's@^ *test +"\$PHP_.*" *= *"no" *&& *PHP_.*=yes *$@#&@g' configure; \
    ./configure --with-unixODBC=shared,/usr; \
    make -j "$(nproc)"; \
    make install; \
    make clean; \
    docker-php-ext-enable odbc; \
    docker-php-ext-install -j "$(nproc)" \
        bcmath \
        bz2 \
        calendar \
        dba \
        enchant \
        exif \
        ffi \
        ftp \
        gd \
        gettext \
        gmp \
        intl \
        ldap \
        mbstring \
        mysqli \
        pcntl \
        pdo_mysql \
        pdo_odbc \
        pdo_pgsql \
        pgsql \
        shmop \
        snmp \
        soap \
        sockets \
        sysvmsg \
        sysvsem \
        sysvshm \
        tidy \
        xsl \
        zip; \
    cd /; \
    docker-php-ext-enable sodium

# PEAR/PECL was removed from PHP 8.6. Build pinned PECL release tarballs directly.
RUN set -eux; \
    install_source_extension() { \
        name="$1"; \
        sourceUrl="$2"; \
        sourceSha256="$3"; \
        shift 3; \
        workDir="$(mktemp -d)"; \
        curl -fsSL --retry 5 --retry-delay 5 -o "$workDir/source.tgz" "$sourceUrl"; \
        if [ -n "$sourceSha256" ]; then \
            echo "$sourceSha256 *$workDir/source.tgz" | sha256sum -c -; \
        fi; \
        mkdir "$workDir/source"; \
        tar -xzf "$workDir/source.tgz" -C "$workDir/source" --strip-components=1; \
        cd "$workDir/source"; \
        phpize; \
        ./configure "$@"; \
        make -j "$(nproc)"; \
        make install; \
        cd /; \
        rm -rf "$workDir"; \
    }; \
    install_pecl_extension() { \
        name="$1"; \
        version="$2"; \
        shift 2; \
        install_source_extension "$name" "https://pecl.php.net/get/${name}-${version}.tgz" "" "$@"; \
    }; \
    # PECL imap 1.0.3 does not compile against PHP 8.6 because it still uses removed XtOffsetOf and INI_STR APIs. \
    # Redis 6.3.0 has the same XtOffsetOf incompatibility; use a checksum-pinned upstream development commit. \
    install_source_extension redis \
        "https://github.com/phpredis/phpredis/archive/${REDIS_SOURCE_COMMIT}.tar.gz" \
        "$REDIS_SOURCE_SHA256"; \
    install_pecl_extension mongodb 2.4.0; \
    # sqlsrv/pdo_sqlsrv 5.13.3 do not compile against PHP 8.6's changed stream and INI APIs. \
    # SQL Server remains available through pdo_odbc/odbc and Microsoft ODBC Driver 18. \
    docker-php-ext-enable --ini-name 20-redis.ini redis; \
    docker-php-ext-enable --ini-name 20-mongodb.ini mongodb

# Xdebug 3.5.3 only supports PHP through 8.5. This pinned, checksum-verified
# upstream 3.6 development commit explicitly accepts PHP < 8.7.
RUN set -eux; \
    workDir="$(mktemp -d)"; \
    curl -fsSL --retry 5 --retry-delay 5 \
        -o "$workDir/xdebug.tar.gz" \
        "https://github.com/xdebug/xdebug/archive/${XDEBUG_SOURCE_COMMIT}.tar.gz"; \
    echo "$XDEBUG_SOURCE_SHA256 *$workDir/xdebug.tar.gz" | sha256sum -c -; \
    mkdir "$workDir/source"; \
    tar -xzf "$workDir/xdebug.tar.gz" -C "$workDir/source" --strip-components=1; \
    cd "$workDir/source"; \
    phpize; \
    ./configure --enable-xdebug; \
    make -j "$(nproc)"; \
    make install; \
    test -f "$(php -r 'echo ini_get("extension_dir");')/xdebug.so"; \
    cd /; \
    rm -rf "$workDir"; \
    rm -rf /usr/local/include/php /usr/local/lib/php/build; \
    rm -f /usr/local/bin/phpize /usr/local/bin/php-config /usr/src/php.tar.xz; \
    find /usr/local -type f -name '*.a' -delete

FROM ${BASE_IMAGE}

ARG AI_CONNECTION_STRING
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING}
ENV PHP_INI_DIR=/usr/local/etc/php
ENV PHP_ORIGIN=php-fpm
ENV NGINX_RUN_USER=www-data
ENV NGINX_DOCUMENT_ROOT=/home/site/wwwroot
ENV NGINX_PORT=8080
ENV XDEBUG_VERSION=3.6-dev-db5e99bf8109ebf6307268fe1ff844001ed47998
ENV LANG=C.UTF-8 LANGUAGE=C.UTF-8 LC_ALL=C.UTF-8
ENV CNB_STACK_ID=oryx.stacks.skeleton
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"
LABEL org.oryx.php.prerelease="true"
LABEL org.oryx.xdebug.version="3.6-dev-db5e99bf8109ebf6307268fe1ff844001ed47998"

# Microsoft does not publish a Resolute feed yet. The official Ubuntu 25.10
# package is unpinned from Ubuntu libraries and is the closest compatible feed.
RUN set -eux; \
    curl -fsSL https://packages.microsoft.com/keys/microsoft-2025.asc \
        | gpg --dearmor -o /usr/share/keyrings/microsoft-2025.gpg; \
    echo 'deb [arch=amd64 signed-by=/usr/share/keyrings/microsoft-2025.gpg] https://packages.microsoft.com/ubuntu/25.10/prod questing main' \
        > /etc/apt/sources.list.d/microsoft-prod.list; \
    apt-get update; \
    ACCEPT_EULA=Y DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        libargon2-1 \
        libbz2-1.0 \
        libcurl4t64 \
        libedit2 \
        libenchant-2-2 \
        libffi8 \
        libfreetype6 \
        libgmp10 \
        libicu78 \
        libjpeg-turbo8 \
        libkrb5-3 \
        libldap2 \
        libodbc2 \
        libodbcinst2 \
        libonig5 \
        libpng16-16t64 \
        libpq5 \
        libreadline8t64 \
        libsodium23 \
        libsnmp40t64 \
        libsqlite3-0 \
        libssl3t64 \
        libtidy58 \
        libxml2-16 \
        libxslt1.1 \
        libzip5 \
        msodbcsql18=18.6.2.1-1 \
        nginx \
        unixodbc \
        zlib1g; \
    rm -rf /var/lib/apt/lists/*

COPY --from=phpbuilder /usr/local /usr/local
COPY --from=startupcmdgen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
COPY images/runtime/php-fpm/8.5/docker-php-entrypoint /usr/local/bin/docker-php-entrypoint
COPY images/runtime/php-fpm/nginx_conf/default.conf /etc/nginx/sites-available/default
COPY images/runtime/php-fpm/nginx_conf/fastcgi.conf /etc/nginx/fastcgi.conf
COPY images/runtime/php-fpm/nginx_conf/proxy_params /etc/nginx/proxy_params
COPY images/runtime/php-fpm/nginx_conf/fastcgi-php.conf /etc/nginx/snippets/fastcgi-php.conf

RUN set -eux; \
    sed -i 's/\r$//' /usr/local/bin/docker-php-entrypoint; \
    chmod +x /usr/local/bin/docker-php-entrypoint; \
    ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx; \
    mkdir -p /home/site/wwwroot /var/www/html /var/cache/nginx; \
    chown www-data:www-data /var/www/html; \
    chmod 1777 /var/www/html; \
    rm -f /etc/nginx/sites-enabled/default; \
    ln -s /etc/nginx/sites-available/default /etc/nginx/sites-enabled/default; \
    sed -ri 's!^user\s+\S+;!user www-data;!' /etc/nginx/nginx.conf; \
    chown -R www-data:www-data /var/cache/nginx; \
    nginx -t

RUN set -eux; \
    cd "${PHP_INI_DIR%/php}"; \
    cp php-fpm.conf.default php-fpm.conf; \
    cp php-fpm.d/www.conf.default php-fpm.d/www.conf; \
    sed -ri 's/^(listen = 127\.0\.0\.1:9000)/;\1/' php-fpm.d/www.conf; \
    { \
        echo '[global]'; \
        echo 'error_log = /proc/self/fd/2'; \
        echo 'log_limit = 8192'; \
        echo; \
        echo '[www]'; \
        echo 'access.log = /proc/self/fd/2'; \
        echo 'clear_env = no'; \
        echo 'catch_workers_output = yes'; \
        echo 'decorate_workers_output = no'; \
        echo 'listen = 9000'; \
    } > php-fpm.d/docker.conf; \
    { \
        echo '[global]'; \
        echo 'daemonize = no'; \
    } > php-fpm.d/zz-docker.conf; \
    { \
        echo 'fastcgi.logging=Off'; \
    } > "$PHP_INI_DIR/conf.d/docker-fpm.ini"; \
    { \
        echo 'opcache.memory_consumption=128'; \
        echo 'opcache.interned_strings_buffer=8'; \
        echo 'opcache.max_accelerated_files=4000'; \
        echo 'opcache.revalidate_freq=60'; \
        echo 'opcache.enable_cli=1'; \
    } > "$PHP_INI_DIR/conf.d/opcache-recommended.ini"; \
    { \
        echo 'error_log=/proc/self/fd/2'; \
        echo 'display_errors=Off'; \
        echo 'log_errors=On'; \
        echo 'display_startup_errors=Off'; \
        echo 'date.timezone=UTC'; \
    } > "$PHP_INI_DIR/conf.d/php.ini"; \
    php --version; \
    php -m | grep -Fx 'Zend OPcache'; \
    ! php -m | grep -i xdebug; \
    ! find "$PHP_INI_DIR/conf.d" -type f -exec grep -Hi xdebug '{}' +; \
    test "$(find /usr/local/lib/php/extensions -name xdebug.so | wc -l)" -eq 1; \
    ! dpkg-query -W -f='${binary:Package}\n' | grep -E -- '(^openssh-(client|server)(:.*)?$|-dev(:.*)?$)'; \
    for tool in gcc g++ make autoconf phpize php-config sftp ssh sshd; do \
        ! command -v "$tool"; \
    done

ENTRYPOINT ["docker-php-entrypoint"]
WORKDIR /home/site/wwwroot
STOPSIGNAL SIGQUIT
EXPOSE 9000
CMD ["php-fpm"]
