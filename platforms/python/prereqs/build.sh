#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

wget https://www.python.org/ftp/python/${PYTHON_VERSION%%[a-z]*}/Python-$PYTHON_VERSION.tar.xz -O /python.tar.xz
wget https://www.python.org/ftp/python/${PYTHON_VERSION%%[a-z]*}/Python-$PYTHON_VERSION.tar.xz.asc -O /python.tar.xz.asc

debianFlavor=$DEBIAN_FLAVOR
pythonSdkFileName=""

if [ "$debianFlavor" == "stretch" ]; then
	# Use default python sdk file name
	pythonSdkFileName=python-$PYTHON_VERSION.tar.gz
else
	pythonSdkFileName=python-$debianFlavor-$PYTHON_VERSION.tar.gz
    # for buster and ubuntu we would need following libraries to build php 
    apt-get update && \
	apt-get upgrade -y && \
    DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
        libssl-dev \
        libncurses5-dev \
        libsqlite3-dev \
        libreadline-dev \
        libbz2-dev \
        libgdm-dev
fi

# Try getting the keys 5 times at most
/tmp/receiveGpgKeys.sh $GPG_KEY
    
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
        --with-system-expat \
        --with-system-ffi \
        --without-ensurepip
fi

make -j $(nproc)

make install

PYTHON_GET_PIP_SHA256="c518250e91a70d7b20cceb15272209a4ded2a0c263ae5776f129e0d9b5674309"
PYTHON_GET_PIP_URL="https://github.com/pypa/get-pip/raw/3cb8888cc2869620f57d5d2da64da38f516078c7/public/get-pip.py"
PYTHON_SETUPTOOLS_VERSION="57.5.0"

# Install pip
wget "$PYTHON_GET_PIP_URL" -O get-pip.py
#echo "$PYTHON_GET_PIP_SHA256 *get-pip.py" | sha256sum -c -
echo "$PYTHON_GET_PIP_SHA256 *get-pip.py" | sha256sum --check --strict -
LD_LIBRARY_PATH=/usr/src/python \
/usr/src/python/python get-pip.py \
    --trusted-host pypi.python.org \
    --prefix $INSTALLATION_PREFIX \
    --disable-pip-version-check \
    --no-cache-dir \
    --no-warn-script-location \
    pip==$PIP_VERSION \
    "setuptools==$PYTHON_SETUPTOOLS_VERSION" 

if [ "${PYTHON_VERSION::1}" == "2" ]; then
    LD_LIBRARY_PATH=$INSTALLATION_PREFIX/lib \
    $INSTALLATION_PREFIX/bin/pip install --no-cache-dir virtualenv
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
sed -i 's|logger\.debug('\''Cleaning up\.\.\.'\'')|logger\.info('\''Cleaning up\.\.\.'\'')|' "$pipReqSetPath"

compressedSdkDir="/tmp/compressedSdk"
mkdir -p $compressedSdkDir
cd "$INSTALLATION_PREFIX"
tar -zcf $compressedSdkDir/$pythonSdkFileName .