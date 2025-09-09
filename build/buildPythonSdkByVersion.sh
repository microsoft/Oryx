#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex
declare -r REPO_DIR=$( cd $( dirname "$0" ) && cd .. && pwd )

version="$1"

buildPythonfromSource()
{
    pythonVersion=$PYTHON_VERSION
    
    if [ ! -z "$1" ]; then
       echo "$1"
       pythonVersion=$1
    fi

    mkdir -p "tmpFiles"
    wget https://www.python.org/ftp/python/${pythonVersion%%[a-z]*}/Python-$pythonVersion.tar.xz -O /tmpFiles/python.tar.xz

    PYTHON_GET_PIP_URL="https://bootstrap.pypa.io/get-pip.py"

    # for buster and ubuntu we would need following libraries
    apt-get update && \
    apt-get upgrade -y && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        build-essential \
        gdb \
        lcov \
        libbluetooth-dev \
        libbz2-dev \
        libffi-dev \
        libgdbm-dev \
        libgdm-dev \
        libgeos-dev \
        liblzma-dev \
        libncurses5-dev \
        libreadline-dev \
        libreadline6-dev \
        libsqlite3-dev \
        libssl-dev \
        lzma \
        lzma-dev \
        pkg-config \
        python3-dev \
        tk-dev \
        uuid-dev \
        zlib1g-dev

    tar -xJf /tmpFiles/python.tar.xz --strip-components=1 -C .

    INSTALLATION_PREFIX=/opt/python/$PYTHON_VERSION


    ./configure \
        --prefix=$INSTALLATION_PREFIX \
        --build=$(dpkg-architecture --query DEB_BUILD_GNU_TYPE) \
        --enable-loadable-sqlite-extensions \
        --enable-shared \
        --with-system-expat \
        --with-system-ffi \
        --without-ensurepip

    make -j $(nproc)

    make install

    export LD_LIBRARY_PATH="/opt/python/$PYTHON_VERSION/lib/"
    $INSTALLATION_PREFIX/bin/python3 --version
    rm -rf /usr/src/python
    find /usr/local -depth \
        \( \
            \( -type d -a \( -name test -o -name tests -o -name idle_test \) \) \
            -o \( -type f -a \( -name '*.pyc' -o -name '*.pyo' -o -name '*.a' \) \) \
        \) -exec rm -rf '{}' + \

    ldconfig

    # make some useful symlinks that are expected to exist in the installation prefix
    cd $INSTALLATION_PREFIX/bin
    ln -s idle3 idle
    ln -s pydoc3 pydoc
    ln -s python3 python
    ln -s python3-config python-config

    # Install pip
    wget "$PYTHON_GET_PIP_URL" -O /tmpFiles/get-pip.py

    $INSTALLATION_PREFIX/bin/python3 /tmpFiles/get-pip.py \
        --trusted-host pypi.python.org \
        --trusted-host pypi.org \
        --trusted-host files.pythonhosted.org \
        --disable-pip-version-check \
        --no-cache-dir \
        --no-warn-script-location

    # Currently only for version '2' of Python, the alias 'python' exists in the 'bin'
    # directory. So to make sure other versions also have this alias, we create the link
    # explicitly here. This is for the scenarios where a user does 'benv python=3.7' and
    # expects the alias 'python' to point to '3.7' rather than '2'. In cases where benv is
    # not passed as an explicit python version, the version '2' is used by default. This is
    # done in the Dockerfile.
    pythonBinDir="$INSTALLATION_PREFIX/bin"
    pythonAliasFile="$pythonBinDir/python"
    if [ ! -e "$pythonAliasFile" ]; then
        IFS='.' read -ra SPLIT_VERSION <<< "$PYTHON_VERSION"
        majorAndMinorParts="${SPLIT_VERSION[0]}.${SPLIT_VERSION[1]}"
        ln -s $pythonBinDir/python$majorAndMinorParts $pythonBinDir/python
    fi

    rm -rf /configure* /config.* /*.txt /*.md /*.rst /*.toml /*.m4 /tmpFiles
    rm -rf /LICENSE /install-sh /Makefile* /pyconfig* /python.tar* /python-* /libpython3.* /setup.py
    rm -rf /Python /PCbuild /Grammar /python /Objects /Parser /Misc /Tools /Programs /Modules /Include /Mac /Doc /PC /Lib 
}

echo
echo "Building python 3.14 or newer from source code..."

IFS='.' read -ra SPLIT_VERSION <<< "$PYTHON_VERSION"

if  [ "${SPLIT_VERSION[0]}" == "3" ] && [ "${SPLIT_VERSION[1]}" -ge "14" ]
then
    buildPythonfromSource $version
else
    source /tmp/oryx/images/installPlatform.sh python $version --dir /opt/python/$version --links false
fi