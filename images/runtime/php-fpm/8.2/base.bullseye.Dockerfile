FROM oryxdevmcr.azurecr.io/private/oryx/php-fpm-8.2-bullseye
SHELL ["/bin/bash", "-c"]
ENV PHP_VERSION 8.2.16

# An environment variable for oryx run-script to know the origin of php image so that
# start-up command can be determined while creating run script
ENV PHP_ORIGIN php-fpm
ENV NGINX_RUN_USER www-data
# Edit the default DocumentRoot setting
ENV NGINX_DOCUMENT_ROOT /home/site/wwwroot
# Install NGINX latest stable version using APT Method with Nginx Repository instead of distribution-provided one:
# - https://www.linuxcapable.com/how-to-install-latest-nginx-mainline-or-stable-on-debian-11/
RUN apt-get update
RUN apt install curl nano -y
RUN curl -sSL https://packages.sury.org/nginx/README.txt | bash -x
RUN apt-get update
RUN yes '' | apt-get install nginx-core nginx-common nginx nginx-full -y
RUN ls -l /etc/nginx
COPY images/runtime/php-fpm/nginx_conf/default.conf /etc/nginx/sites-available/default
COPY images/runtime/php-fpm/nginx_conf/default.conf /etc/nginx/sites-enabled/default
RUN sed -ri -e 's!worker_connections 768!worker_connections 10068!g' /etc/nginx/nginx.conf
RUN sed -ri -e 's!# multi_accept on!multi_accept on!g' /etc/nginx/nginx.conf
RUN ls -l /etc/nginx
RUN nginx -t
# Edit the default port setting
ENV NGINX_PORT 8080

# Install common PHP extensions
# TEMPORARY: Holding odbc related packages from upgrading.
RUN apt-mark hold msodbcsql18 odbcinst1debian2 odbcinst unixodbc unixodbc-dev \
    && apt-get update \
    && apt-get upgrade -y \
    && ln -s /usr/lib/x86_64-linux-gnu/libldap.so /usr/lib/libldap.so \
    && ln -s /usr/lib/x86_64-linux-gnu/liblber.so /usr/lib/liblber.so \
    && ln -s /usr/include/x86_64-linux-gnu/gmp.h /usr/include/gmp.h

RUN set -eux; \
    if [[ $PHP_VERSION == 7.4.* || $PHP_VERSION == 8.0.* || $PHP_VERSION == 8.1.*  || $PHP_VERSION == 8.2.* ]]; then \
		apt-get update \
        && apt-get upgrade -y \
        && apt-get install -y --no-install-recommends apache2-dev \
        && docker-php-ext-configure gd --with-freetype --with-jpeg \
        && PHP_OPENSSL=yes docker-php-ext-configure imap --with-kerberos --with-imap-ssl ; \
    else \
		docker-php-ext-configure gd --with-png-dir=/usr --with-jpeg-dir=/usr \
        && docker-php-ext-configure imap --with-kerberos --with-imap-ssl ; \
    fi

RUN docker-php-ext-configure pdo_odbc --with-pdo-odbc=unixODBC,/usr \
    && docker-php-ext-install gd \
        mysqli \
        opcache \
        pdo \
        pdo_mysql \
        pdo_pgsql \
        pgsql \
        ldap \
        intl \
        gmp \
        zip \
        bcmath \
        mbstring \
        pcntl \
        calendar \
        exif \
        gettext \
        imap \
        tidy \
        shmop \
        soap \
        sockets \
        sysvmsg \
        sysvsem \
        sysvshm \
        pdo_odbc \
# deprecated from 7.4, so should be avoided in general template for all php versions
#        xmlrpc \
        xsl
RUN pecl install redis && docker-php-ext-enable redis
# https://github.com/Imagick/imagick/issues/331
RUN pecl install imagick && docker-php-ext-enable imagick

# deprecated from 5.*, so should be avoided 	
RUN set -eux; \	
    if [[ $PHP_VERSION != 5.* && $PHP_VERSION != 7.0.* ]]; then \	
        pecl install mongodb && docker-php-ext-enable mongodb; \	
    fi	

# https://github.com/microsoft/mysqlnd_azure, Supports  7.2*, 7.3* and 7.4*
RUN set -eux; \
    if [[ $PHP_VERSION == 7.2.* || $PHP_VERSION == 7.3.* || $PHP_VERSION == 7.4.* ]]; then \
        echo "pecl/mysqlnd_azure requires PHP (version >= 7.2.*, version <= 7.99.99)"; \
        pecl install mysqlnd_azure \
        && docker-php-ext-enable mysqlnd_azure; \
    fi

# Install the Microsoft SQL Server PDO driver on supported versions only.
#  - https://docs.microsoft.com/en-us/sql/connect/php/installation-tutorial-linux-mac
#  - https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server
# For php|8.0, latest stable version of pecl/sqlsrv, pecl/pdo_sqlsrv is 5.11.0
RUN set -eux; \
    if [[ $PHP_VERSION == 8.0.* ]]; then \
        pecl install sqlsrv-5.11.0 pdo_sqlsrv-5.11.0 \
        && echo extension=pdo_sqlsrv.so >> $(php --ini | grep "Scan for additional .ini files" | sed -e "s|.*:\s*||")/30-pdo_sqlsrv.ini \
        && echo extension=sqlsrv.so >> $(php --ini | grep "Scan for additional .ini files" | sed -e "s|.*:\s*||")/20-sqlsrv.ini; \
    fi

# Latest pecl/sqlsrv, pecl/pdo_sqlsrv requires PHP (version >= 8.1.0)
RUN set -eux; \
    if [[ $PHP_VERSION == 8.1.* || $PHP_VERSION == 8.2.* ]]; then \
        pecl install sqlsrv pdo_sqlsrv \
        && echo extension=pdo_sqlsrv.so >> `php --ini | grep "Scan for additional .ini files" | sed -e "s|.*:\s*||"`/30-pdo_sqlsrv.ini \
        && echo extension=sqlsrv.so >> `php --ini | grep "Scan for additional .ini files" | sed -e "s|.*:\s*||"`/20-sqlsrv.ini; \
    fi

RUN { \
                echo 'opcache.memory_consumption=128'; \
                echo 'opcache.interned_strings_buffer=8'; \
                echo 'opcache.max_accelerated_files=4000'; \
                echo 'opcache.revalidate_freq=60'; \
                echo 'opcache.fast_shutdown=1'; \
                echo 'opcache.enable_cli=1'; \
    } > /usr/local/etc/php/conf.d/opcache-recommended.ini

# NOTE: zend_extension=opcache is already configured via docker-php-ext-install, above
RUN { \
                echo 'error_log=/var/log/apache2/php-error.log'; \
                echo 'display_errors=Off'; \
                echo 'log_errors=On'; \
                echo 'display_startup_errors=Off'; \
                echo 'date.timezone=UTC'; \
    } > /usr/local/etc/php/conf.d/php.ini

RUN set -x \
    && docker-php-source extract \
    && cd /usr/src/php/ext/odbc \
    && phpize \
    && sed -ri 's@^ *test +"\$PHP_.*" *= *"no" *&& *PHP_.*=yes *$@#&@g' configure \
    && chmod +x ./configure \
    && ./configure --with-unixODBC=shared,/usr \
    && docker-php-ext-install odbc \
    && rm -rf /var/lib/apt/lists/*

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"