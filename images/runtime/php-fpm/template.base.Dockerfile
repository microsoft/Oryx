FROM %PHP_BASE_IMAGE%
SHELL ["/bin/bash", "-c"]
ENV PHP_VERSION %PHP_VERSION%

# An environment variable for oryx run-script to know the origin of php image so that
# start-up command can be determined while creating run script
ENV PHP_ORIGIN php-fpm
ENV NGINX_RUN_USER www-data
# Edit the default DocumentRoot setting
ENV NGINX_DOCUMENT_ROOT /home/site/wwwroot
# Configure NGINX 
#RUN apt-get update \
#    && apt-get install nginx -y --no-install-recommends
RUN ls -l /etc/nginx
COPY images/runtime/php-fpm/nginx_conf/default.conf /etc/nginx/sites-available/default
COPY images/runtime/php-fpm/nginx_conf/default.conf /etc/nginx/sites-enabled/default
RUN sed -ri -e 's!worker_connections 768!worker_connections 10068!g' /etc/nginx/nginx.conf
RUN sed -ri -e 's!# multi_accept on!multi_accept on!g' /etc/nginx/nginx.conf
RUN ls -l /etc/nginx
# Edit the default port setting
ENV NGINX_PORT 8080

# Install common PHP extensions
RUN apt-get update \
    && apt-get upgrade -y \
    && ln -s /usr/lib/x86_64-linux-gnu/libldap.so /usr/lib/libldap.so \
    && ln -s /usr/lib/x86_64-linux-gnu/liblber.so /usr/lib/liblber.so \
    && ln -s /usr/include/x86_64-linux-gnu/gmp.h /usr/include/gmp.h

RUN set -eux; \
    if [[ $PHP_VERSION == 7.4.* ]]; then \
        docker-php-ext-configure gd --with-png=/usr --with-jpeg=/usr ; \
    else \
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
RUN pecl install sqlsrv pdo_sqlsrv \
        && echo extension=pdo_sqlsrv.so >> `php --ini | grep "Scan for additional .ini files" | sed -e "s|.*:\s*||"`/30-pdo_sqlsrv.ini \
        && echo extension=sqlsrv.so >> `php --ini | grep "Scan for additional .ini files" | sed -e "s|.*:\s*||"`/20-sqlsrv.ini;

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
