This document describes how **Python** apps are detected and built. It includes
details on components and configuration of build and run images too.

# Contents

- [Contents](#contents)
- [Base image](#base-image)
  - [System packages](#system-packages)
- [Detect](#detect)
- [Build](#build)
  - [Package manager](#package-manager)
- [Run](#run)
    - [Gunicorn multiple workers suport](#gunicorn-multiple-workers-suport)
- [Version support](#version-support)

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

1. If user has specified a start script, run it.
1. Else, find a WSGI module and run with [gunicorn][].
    1. Look for and run a directory containing a `wsgi.py` file (*for Django*).
    1. Look for the following files in the root of the repo and an `app` class within them (*for Flask* and other WSGI frameworks).
        * `application.py`
        * `app.py`
        * `index.py`
        * `server.py`

### Gunicorn multiple workers suport

To enable running gunicorn with multiple [workers strategy][] and fully utilize the cores to improve performance
and prevent potential timeout/blocks from sync workers, add and set the environment variable `PYTHON_ENABLE_GUNICORN_MULTIWORKERS=true` into the app settings.

In Azure Web Apps the version of the Python runtime which runs your app is
determined by the value of `LinuxFxVersion` in your [site config][]. See
[../base\_images.md](../base_images.md#azure-web-apps-runtimes-and-versions)
for how to modify this.

[gunicorn]: https://gunicorn.org/
[site config]: https://docs.microsoft.com/en-us/rest/api/appservice/webapps/get#siteconfig
[workers strategy]: https://docs.gunicorn.org/en/stable/design.html#how-many-workers

# Version support

The Python project uses this [release schedule][], clarified further in
["Development Lifecycle"][]. Oryx will support all releases in `bugfix`
status, i.e. `3.7.x` and `2.7.x` in early 2019; and 3.6 and later releases in
`security` status.

We will provide notification twelve months prior to removing a release line;
subscribe to [Azure Updates][] to stay updated!

We will update the `patch` version of supported `major.minor` releases at
least once every 3 months, replacing the previous `patch` version for that
release.

[release schedule]: https://devguide.python.org/#status-of-python-branches
["Development Lifecycle"]: https://devguide.python.org/devcycle/#devcycle
[Azure Updates]: https://azure.microsoft.com/updates/
