FROM %PHP_BASE_IMAGE%
SHELL ["/bin/bash", "-c"]
ENV PHP_VERSION %PHP_VERSION%

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

# Install common PHP extensions
RUN apt-get update \
    && apt-get upgrade -y \
    && ln -s /usr/lib/x86_64-linux-gnu/libldap.so /usr/lib/libldap.so \
    && ln -s /usr/lib/x86_64-linux-gnu/liblber.so /usr/lib/liblber.so \
    && ln -s /usr/include/x86_64-linux-gnu/gmp.h /usr/include/gmp.h

RUN set -eux; \
    if [[ $PHP_VERSION == 7.4.* ]]; then \
		echo "hellow: "$PHP_VERSION
        docker-php-ext-configure gd --with-freetype --with-jpeg ; \
    else \
		echo "it's not php 7.4"
		echo $PHP_VERSION
        docker-php-ext-configure gd --with-png-dir=/usr --with-jpeg-dir=/usr ; \
    fi

RUN docker-php-ext-configure imap --with-kerberos --with-imap-ssl \
    && docker-php-ext-configure pdo_odbc --with-pdo-odbc=unixODBC,/usr \
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
        wddx \
        xmlrpc \
        xsl \
    && pecl install imagick && docker-php-ext-enable imagick \
    && pecl install mongodb && docker-php-ext-enable mongodb

# Install the Microsoft SQL Server PDO driver on supported versions only.
#  - https://docs.microsoft.com/en-us/sql/connect/php/installation-tutorial-linux-mac
#  - https://docs.microsoft.com/en-us/sql/connect/odbc/linux-mac/installing-the-microsoft-odbc-driver-for-sql-server
RUN set -eux; \
    if [[ $PHP_VERSION == 7.1.* || $PHP_VERSION == 7.2.* || $PHP_VERSION == 7.3.* || $PHP_VERSION == 7.4.* ]]; then \
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

RUN { \
                echo 'error_log=/var/log/apache2/php-error.log'; \
                echo 'display_errors=Off'; \
                echo 'log_errors=On'; \
                echo 'display_startup_errors=Off'; \
                echo 'date.timezone=UTC'; \
                echo 'zend_extension=opcache'; \
    } > /usr/local/etc/php/conf.d/php.ini

RUN set -x \
    && docker-php-source extract \
    && cd /usr/src/php/ext/odbc \
    && phpize \
    && sed -ri 's@^ *test +"\$PHP_.*" *= *"no" *&& *PHP_.*=yes *$@#&@g' configure \
    && chmod +x ./configure \
    && ./configure --with-unixODBC=shared,/usr \
    && docker-php-ext-install odbc

RUN rm -rf /tmp/oryx