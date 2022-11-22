This document describes how **PHP** apps are detected and built. It includes
details on components and configuration of build and run images too.

# Contents

- [Contents](#contents)
- [Base image](#base-image)
  - [System packages](#system-packages)
- [Detect](#detect)
- [Build](#build)
  - [Package manager](#package-manager)
- [Run](#run)
- [Version support](#version-support)

# Base image

PHP runtime images are layered on Docker's [official PHP
images](https://github.com/docker-library/php).

## System packages

The Apache HTTP Server  is used as the application server.
The following PHP extensions are installed & enabled in the runtime images:

* gd
* imagick
* mysqli
* opcache
* odbc
* sqlsrv
* pdo
* pdo_sqlsrv
* pdo_mysql
* pdo_pgsql
* pgsql
* ldap
* intl
* gmp
* zip
* bcmath
* mbstring
* pcntl
* calendar
* exif
* gettext
* imap
* redis
* tidy
* shmop
* soap
* sockets
* sysvmsg
* sysvsem
* sysvshm
* pdo_odbc
* wddx
* xmlrpc
* xsl

# Detect

The PHP toolset will run when the following conditions are met:
1. `composer.json` file exists in the root of the repository.

2. Files with `.php` extension in root of repository or in any sub-directories.

# Build

The following process is applied for each build:

1. Run custom script if specified by `PRE_BUILD_SCRIPT_PATH`.
1. Run `php composer.phar install --ignore-platform-reqs --no-interaction` if composer file found.
1. Run custom script if specified by `POST_BUILD_SCRIPT_PATH`.

## Package manager

The latest version of *Composer* is used to install dependencies.

# Run

The following process is applied to determine how to start an app:

1. If user has specified a start script, run it.
1. Else, run `apache2-foreground`.

[Composer]: https://getcomposer.org/

# Version support

The PHP project defines this [release schedule][]. Oryx supports all actively supported
releases (7.2, 7.3, 7.4, 8.0, 8.1, 8.2), in addition to 5.6 & 7.0.

We will update the `patch` version of a release at least once every 3 months,
replacing the previous `patch` version for that release.

[release schedule]: https://www.php.net/supported-versions.php
