This document describes how **Python** apps are detected and built. It includes
details on components and configuration of build and run images too.

# Contents

- [Contents](#contents)
- [Base image](#base-image)
  - [System packages](#system-packages)
- [Detect](#detect)
  - [Detect Conda environment and Python JupyterNotebook](#detect-conda-environment-and-python-jupyternotebook)
- [Build](#build)
  - [Build Conda environment and Python JupyterNotebook](#build-conda-environment-and-python-jupyternotebook)
  - [Package manager](#package-manager)
- [Run](#run)
    - [Startup environment script](#startup-environment-script)
    - [Gunicorn multiple workers support](#gunicorn-multiple-workers-support)
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
* libexpat1
* unzip
* libodbc1
* apt-transport-https
* swig3.0
* locales

gunicorn (a Python package) is also included.

# Detect

The Python toolset is run when any of the following conditions are met:

* `requirements.txt` in root of repo
* `runtime.txt` in root of repo
* Files with `.py` extension in root of repo or in sub-directories if set `DISABLE_RECURSIVE_LOOKUP=false`.
* `requirements.txt` at specific path within the repo if set `CUSTOM_REQUIREMENTSTXT_PATH`.

## Detect Conda environment and Python JupyterNotebook

The Python conda is run when any of the following conditions are met:

* Conda environment file `environment.yml` or `environment.yaml` is found in root of repo
* Files with `.ipynb` extension in root of repo

# Build

The following process is applied for each build.

1. Run custom command or script if specified by `PRE_BUILD_COMMAND` or `PRE_BUILD_SCRIPT_PATH`.
2. Create python virtual environment if specified by `VIRTUALENV_NAME`.
3. Run `python -m pip install --cache-dir /usr/local/share/pip-cache --prefer-binary -r requirements.txt` 
   if `requirements.txt` exists in the root of repo or specified by `CUSTOM_REQUIREMENTSTXT_PATH`.
4. Run `python setup.py install` if `setup.py` exists.
5. Run python package commands and determine python package wheel.
6. If `manage.py` is found in the root of the repo `manage.py collectstatic` is run. However,
   if `DISABLE_COLLECTSTATIC` is set to `true` this step is skipped.
7. Compress virtual environment folder if specified by `compress_virtualenv` property key.
8. Run custom command or script if specified by `POST_BUILD_COMMAND` or `POST_BUILD_SCRIPT_PATH`.

## Build Conda environment and Python JupyterNotebook

The following process is applied for each build.

1. Run custom command or script if specified by `PRE_BUILD_COMMAND` or `PRE_BUILD_SCRIPT_PATH`.
2. Set up Conda virtual environemnt `conda env create --file $envFile`.
3. If `requirment.txt` exists in the root of repo or specified by `CUSTOM_REQUIREMENTSTXT_PATH`, activate environemnt 
  `conda activate $environmentPrefix` and run `pip install --no-cache-dir -r requirements.txt`.
4. Run custom command or script if specified by `POST_BUILD_COMMAND` or `POST_BUILD_SCRIPT_PATH`.


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

### Startup environment script

A host can optionally pass `-startupEnvironmentScript /absolute/path/setup.sh`
to the Python startup generator's `oryx create-script` command. The path must
be an absolute POSIX path. It is treated literally, including spaces, quotes,
and shell metacharacters, and need not exist when the startup script is
generated.

When this option is set, the generated `/bin/sh` script prepares `APP_PATH`,
`PATH`, and the Python package or virtual environment, then sources the supplied
script in the current shell before the configured `PRE_RUN_COMMAND` and the
application command. This applies to default, custom, and debug startup commands.
Exports from the setup script are available to both subsequent commands. The
setup script must be readable at runtime and compatible with POSIX `sh`; it does
not need to be executable. A missing or unreadable script stops startup. A
nonzero status returned by the script stops startup with that status, before
the pre-run or application command runs. The script should report any setup
failure and return a nonzero status itself.

Only pass a trusted setup script: sourcing it gives it the same privileges as
the startup shell. This hook is an environment-setup ordering point, not a
boundary before all application-supplied code. Existing dynamic installation
and virtual environment activation still precede it and must also be trusted.
Compressed application output is extracted during script generation; compressed
virtual environments are extracted by the generated script as before. Python
version probes use `-I -S` when the option is set so that they do not load
`sitecustomize`, `usercustomize`, or site-package `.pth` files before the hook.
The selected Python interpreter itself must be trusted.

If the option is omitted or empty, generated startup scripts retain their
existing behavior, including the original placement of `PRE_RUN_COMMAND`
before Python package setup.

### Gunicorn multiple workers support

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
