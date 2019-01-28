This document describes how **Python** apps are detected and built. It includes
details on components and configuration of build and run images too.

# Contents

1. [Base Image](#base-image)
    * [System packages](#system-packages)
1. [Detect](#detect)
1. [Build](#build)
    * [Package manager](#package-manager)
1. [Run](#run)

# Base image

Python runtime images are layered on Docker's [official Python
image](https://github.com/docker-library/python).

## System packages

The following system packages are added to the runtime image:

* curl
* gnupg
* libpq-dev
* default-libmysqlclient-dev
* unixodbc-dev
* msodbcsql17

gunicorn (a Python package) is also included.

# Detect

The Python toolset is run when the following conditions are met:

1. `requirements.txt` in root of repo
1. `runtime.txt` in root of repo
1. Files with `.py` extension in root of repo.

# Build

The following process is applied for each build.

1. Run custom script if specified by `PRE_BUILD_SCRIPT_PATH`.
1. Run `pip install -r requirements.txt`.
1. If `manage.py` is found in the root of the repo `manage.py collectstatic` is run. However,
   if `DISABLE_COLLECTSTATIC` is set to `true` this step is skipped.
1. Run custom script if specified by `POST_BUILD_SCRIPT_PATH`.

## Package manager

The latest version of `pip` is used to install dependencies.

# Run

The following process is applied to determine how to start an app.

1. If user has specified a start script utilize that.
1. Find a WSGI module and run with [gunicorn][].
    1. Look for and run a directory containing a `wsgi.py` file (*for Django*).
    1. Look for the following files in the root of the repo and an `app` class within them (*for Flask* and other WSGI frameworks).
        * `application.py`
        * `app.py`
        * `index.py`
        * `server.py`

In Azure Web Apps the version of the Python runtime which runs your app is
determined by the value of `LinuxFxVersion` in your [site config][]. See
[../base\_images.md](../base_images.md#azure-web-apps-runtimes-and-versions)
for how to modify this.

[gunicorn]: https://gunicorn.org/
[site config]: https://docs.microsoft.com/en-us/rest/api/appservice/webapps/get#siteconfig
