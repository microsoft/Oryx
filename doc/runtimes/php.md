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

PHP runtime images compile PHP from source, using the versions and SHA256
checksums in `images/constants.yml`. The build and extension helpers are based
on Docker's [official PHP images](https://github.com/docker-library/php).

Apache (mod_php) images are available for PHP 8.2 on Bullseye and PHP 8.3 on
Bookworm. They use `images/runtime/commonbase/phpRuntimeBase.Dockerfile` on top
of the generic Oryx runtime base, followed by
`images/runtime/php/{version}/{osFlavor}.Dockerfile`. Publishing names the
intermediate base `php-run-base-{osFlavor}`. FPM images remain separate.

From the repository root on a Linux Docker host with Mike Farah's `yq` v4:

```bash
bash images/runtime/build_runtime_images.sh php 8.2 bullseye
bash images/runtime/build_runtime_images.sh php 8.3 bookworm
```

Runtime smoke tests live in `PhpApacheImageTest`. After tagging the built
images as `${ORYX_TEST_IMAGE_BASE}/php:8.2-debian-bullseye${ORYX_TEST_TAG_SUFFIX}`
and `${ORYX_TEST_IMAGE_BASE}/php:8.3-debian-bookworm${ORYX_TEST_TAG_SUFFIX}`, run:

```bash
dotnet test tests/Oryx.RuntimeImage.Tests/Oryx.RuntimeImage.Tests.csproj --filter FullyQualifiedName~PhpApacheImageTest
```

The tests verify the exact PHP version served by mod_php, extensions, custom
document root and port via the startup generator, `.htaccess` rewrite, and
denial of HTTP access to `.git`.

The runtime Dockerfiles accept `BASE_IMAGE`, `PHP_VERSION`, `PHP_SHA256`,
`GIT_COMMIT`, `BUILD_NUMBER`, `RELEASE_TAG_NAME`, and `AI_CONNECTION_STRING`.
Optional `PHP_SOURCE_BASE_URL` selects a source mirror; SHA256 verification is
mandatory even with a mirror. Official downloads also verify the PHP release
signature. Apache and FPM use the same patch versions; currently these are
8.2.35 and 8.3.36.

## System packages

The Apache variants use prefork and mod_php, not FPM or NGINX.
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
* xsl

Imagick is excluded on PHP 8.3, matching the FPM image. SQL Server extensions
are pinned to 5.12.0 on PHP 8.2; PHP 8.3 uses the current compatible release.
The removed wddx and xmlrpc extensions are not installed.

# Detect

The PHP toolset will run when the following conditions are met:
1. `composer.json` file exists in the root of the repository.

2. Files with `.php` extension in root of repository or in any sub-directories.

# Build

The following process is applied for each build:

1. Run custom command or script if specified by `PRE_BUILD_COMMAND` or `PRE_BUILD_SCRIPT_PATH`.
1. Run `php composer.phar install --ignore-platform-reqs --no-interaction` if composer file found.
1. Run custom command or script if specified by `POST_BUILD_COMMAND` or `POST_BUILD_SCRIPT_PATH`.

## Package manager

The latest version of *Composer* is used to install dependencies.

# Run

The following process is applied to determine how to start an app:

1. If user has specified a start script, run it.
1. Else, run `apache2-foreground`.

Apache listens on `APACHE_PORT` (default `8080`) and serves
`APACHE_DOCUMENT_ROOT` (default `/home/site/wwwroot`). `.htaccess` overrides
and rewrite are enabled. The Oryx startup generator sets these values for the
application; `PHP_ORIGIN=php` keeps it on the Apache startup path. Logs are
directed to stdout/stderr and graceful shutdown uses `SIGWINCH`.

[Composer]: https://getcomposer.org/

# Version support

The PHP project defines this [release schedule][]. Oryx supports all [actively supported releases](https://www.php.net/supported-versions.php).

We will update the `patch` version of a release at least once every 3 months,
replacing the previous `patch` version for that release.

[release schedule]: https://www.php.net/supported-versions.php
