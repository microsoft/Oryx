FROM oryxdevmcr.azurecr.io/private/oryx/php-8.0-buster
SHELL ["/bin/bash", "-c"]
ENV PHP_VERSION 8.0.30

RUN a2enmod rewrite expires include deflate remoteip headers

ENV APACHE_RUN_USER www-data
# Edit the default DocumentRoot setting
ENV APACHE_DOCUMENT_ROOT /home/site/wwwroot
RUN sed -ri -e 's!/var/www/html!${APACHE_DOCUMENT_ROOT}!g' /etc/apache2/sites-available/*.conf
RUN sed -ri -e 's!/var/www/!${APACHE_DOCUMENT_ROOT}!g' /etc/apache2/apache2.conf /etc/apache2/conf-available/*.conf
# Edit the default port setting
ENV APACHE_PORT 8080
RUN sed -ri -e 's!<VirtualHost \*:80>!<VirtualHost *:${APACHE_PORT}>!g' /etc/apache2/sites-available/*.conf
RUN sed -ri -e 's!<VirtualHost _default_:443>!<VirtualHost _default_:${APACHE_PORT}>!g' /etc/apache2/sites-available/*.conf
RUN sed -ri -e 's!Listen 80!Listen ${APACHE_PORT}!g' /etc/apache2/ports.conf
# Edit Configuration to instruct Apache on how to process PHP files
RUN echo -e '<FilesMatch "\.(?i:ph([[p]?[0-9]*|tm[l]?))$">\n SetHandler application/x-httpd-php\n</FilesMatch>' >> /etc/apache2/apache2.conf
# Disable Apache2 server signature
RUN echo -e 'ServerSignature Off' >> /etc/apache2/apache2.conf
RUN echo -e 'ServerTokens Prod' >> /etc/apache2/apache2.conf
RUN { \
   echo '<DirectoryMatch "^/.*/\.git/">'; \
   echo '   Order deny,allow'; \
   echo '   Deny from all'; \
   echo '</DirectoryMatch>'; \
} >> /etc/apache2/apache2.conf

# Install common PHP extensions
# TEMPORARY: Holding odbc related packages from upgrading.
RUN apt-mark hold msodbcsql18=18.3.3.1-1 odbcinst1debian2 odbcinst unixodbc unixodbc-dev \
    && apt-get update \
    && apt-get upgrade -y \
    && ln -s /usr/lib/x86_64-linux-gnu/libldap.so /usr/lib/libldap.so \
    && ln -s /usr/lib/x86_64-linux-gnu/liblber.so /usr/lib/liblber.so \
    && ln -s /usr/include/x86_64-linux-gnu/gmp.h /usr/include/gmp.h

RUN set -eux; \
    if [[ $PHP_VERSION == 7.4.* || $PHP_VERSION == 8.0.* || $PHP_VERSION == 8.1.* || $PHP_VERSION == 8.2.* ]]; then \
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
#       wddx \
#       xmlrpc \
        xsl

RUN set -eux; \
    if [[ $PHP_VERSION != 5.* ]]; then \
        pecl install redis && docker-php-ext-enable redis; \
    fi

# https://github.com/Imagick/imagick/issues/331
RUN pecl install imagick && docker-php-ext-enable imagick

# deprecated from 5.*, so should be avoided 
RUN set -eux; \
    if [[ $PHP_VERSION != 5.* && $PHP_VERSION != 7.0.* ]]; then \
        echo "pecl/mongodb requires PHP (version >= 7.1.0, version <= 7.99.99)"; \
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

RUN rm -rf /tmp/oryx

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"