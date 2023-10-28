#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

pythonVersion=$PYTHON_VERSION 

wget https://www.python.org/ftp/python/${pythonVersion%%[a-z]*}/Python-$pythonVersion.tar.xz -O /python.tar.xz
wget https://www.python.org/ftp/python/${pythonVersion%%[a-z]*}/Python-$pythonVersion.tar.xz.asc -O /python.tar.xz.asc

debianFlavor=$DEBIAN_FLAVOR
debianHackFlavor=$DEBIAN_HACK_FLAVOR
gpgKey=$GPG_KEY

pythonSdkFileName=""
PYTHON_GET_PIP_URL="https://github.com/pypa/get-pip/raw/3cb8888cc2869620f57d5d2da64da38f516078c7/public/get-pip.py"

# For buster and ubuntu we would need following libraries.
# We also add all optional python modules:
# https://devguide.python.org/getting-started/setup-building/index.html#install-dependencies
    apt-get update && \
	apt-get upgrade -y && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        libssl-dev \
        libncurses5-dev \
        libsqlite3-dev \
        libreadline-dev \
        libbz2-dev \
        libgdm-dev \
        libbluetooth-dev \
        tk-dev \
        uuid-dev \
        build-essential \
        gdb \
        lcov \
        pkg-config \
        libffi-dev \
        libgdbm-dev \
        liblzma-dev \
        libsqlite3-dev \
        lzma \
        lzma-dev \
        zlib1g-dev

PYTHON_GET_PIP_URL="https://bootstrap.pypa.io/get-pip.py"

if [ "$debianFlavor" == "stretch" ]; then
	# Use default python sdk file name
    echo "Hack flavor is: "$debianHackFlavor

    pythonSdkFileName=python-$PYTHON_VERSION.tar.gz
    
    if [[ $PYTHON_VERSION == 3.6* ]]; then
        PYTHON_GET_PIP_URL="https://bootstrap.pypa.io/pip/3.6/get-pip.py"
    fi

    PIP_VERSION="20.2.3"
else
	pythonSdkFileName=python-$debianFlavor-$PYTHON_VERSION.tar.gz
fi

# Try getting the keys 5 times at most
/tmp/receiveGpgKeys.sh $gpgKey

gpg --batch --verify /python.tar.xz.asc /python.tar.xz
tar -xJf /python.tar.xz --strip-components=1 -C .

INSTALLATION_PREFIX=/opt/python/$PYTHON_VERSION

if [ "${PYTHON_VERSION::1}" == "2" ]; then
    ./configure \
        --prefix=$INSTALLATION_PREFIX \
        --build=$(dpkg-architecture --query DEB_BUILD_GNU_TYPE) \
        --enable-shared \
        --enable-unicode=ucs4
else
    ./configure \
        --prefix=$INSTALLATION_PREFIX \
        --build=$(dpkg-architecture --query DEB_BUILD_GNU_TYPE) \
        --enable-loadable-sqlite-extensions \
        --enable-shared \
	--enable-optimizations \
	--with-lto \
        --with-system-expat \
        --with-system-ffi \
        --without-ensurepip
fi

make -j $(nproc)

make install

IFS='.' read -ra SPLIT_VERSION <<< "$PYTHON_VERSION"
if [ "${SPLIT_VERSION[0]}" == "3" ] && [ "${SPLIT_VERSION[1]}" -ge "10" ]
then
    rm -rf /usr/src/python
    find /usr/local -depth \
        \( \
            \( -type d -a \( -name test -o -name tests -o -name idle_test \) \) \
            -o \( -type f -a \( -name '*.pyc' -o -name '*.pyo' -o -name '*.a' \) \) \
        \) -exec rm -rf '{}' + \

    ldconfig
    python3 --version

    # make some useful symlinks that are expected to exist
    cd /usr/local/bin
    ln -s idle3 idle
    ln -s pydoc3 pydoc
    ln -s python3 python
    ln -s python3-config python-config

    PYTHON_GET_PIP_SHA256="c518250e91a70d7b20cceb15272209a4ded2a0c263ae5776f129e0d9b5674309"

    # Install pip
    wget "$PYTHON_GET_PIP_URL" -O get-pip.py

    python3 get-pip.py \
        --trusted-host pypi.python.org \
        --trusted-host pypi.org \
        --trusted-host files.pythonhosted.org \
        --disable-pip-version-check \
        --no-cache-dir \
        --no-warn-script-location

    rm -rf /configure* /config.* /*.txt /*.md /*.rst /*.toml /*.m4 /tmpFiles
    rm -rf /LICENSE /install-sh /Makefile* /pyconfig* /python.tar* /python-* /libpython3.* /setup.py
    rm -rf /Python /PCbuild /Grammar /python /Objects /Parser /Misc /Tools /Programs /Modules /Include /Mac /Doc /PC /Lib 
else
    # Install pip
    wget "$PYTHON_GET_PIP_URL" -O get-pip.py

    LD_LIBRARY_PATH=/usr/src/python \
    /usr/src/python/python get-pip.py \
        --trusted-host pypi.python.org \
        --trusted-host pypi.org \
        --trusted-host files.pythonhosted.org \
        --prefix $INSTALLATION_PREFIX \
        --disable-pip-version-check \
        --no-cache-dir \
        --no-warn-script-location \
        pip==$PIP_VERSION

    if [ "${PYTHON_VERSION::1}" == "2" ]; then
        LD_LIBRARY_PATH=$INSTALLATION_PREFIX/lib \
        $INSTALLATION_PREFIX/bin/pip install --no-cache-dir virtualenv
    fi
fi

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

echo
echo "Contents of '$pythonBinDir':"
ls -l $pythonBinDir
echo

# Replace log level in pip's code as a workaround for https://github.com/pypa/pip/issues/6189
pipReqSetPath=`find $INSTALLATION_PREFIX/lib -path "*site-packages/pip/_internal/req/req_set.py"`
if [ -n $pipReqSetPath ]; then
    sed -i 's|logger\.debug('\''Cleaning up\.\.\.'\'')|logger\.info('\''Cleaning up\.\.\.'\'')|' "$pipReqSetPath"
fi

compressedSdkDir="/tmp/compressedSdk"
mkdir -p $compressedSdkDir
cd "$INSTALLATION_PREFIX"
tar -zcf $compressedSdkDir/$pythonSdkFileName .
