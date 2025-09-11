#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

pythonVersion=$PYTHON_VERSION 
osFlavor=$OS_FLAVOR
python_sha=$PYTHON_SHA256
gpgKey=$GPG_KEY

wget https://www.python.org/ftp/python/${pythonVersion%%[a-z]*}/Python-$pythonVersion.tar.xz -O /python.tar.xz

if [ -n "$python_sha" ]; then
    echo "Verifying Python source code using SHA256 checksum..."
    echo "$python_sha /python.tar.xz" | sha256sum -c -
    echo "SHA256 verification successful!"
fi

if [ -n "$gpgKey" ]; then
  wget https://www.python.org/ftp/python/${pythonVersion%%[a-z]*}/Python-$pythonVersion.tar.xz.asc -O /python.tar.xz.asc

  # Try getting the keys 5 times at most
  /tmp/receiveGpgKeys.sh $gpgKey
  gpg --batch --verify /python.tar.xz.asc /python.tar.xz
fi


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

PIP_VERSION="21.2.4"
pythonSdkFileName=python-$osFlavor-$PYTHON_VERSION.tar.gz

tar -xJf /python.tar.xz --strip-components=1 -C .

INSTALLATION_PREFIX=/opt/python/$PYTHON_VERSION

./configure \
    --prefix=$INSTALLATION_PREFIX \
    --build=$(dpkg-architecture --query DEB_BUILD_GNU_TYPE) \
    --enable-loadable-sqlite-extensions \
    --enable-shared \
    --enable-optimizations \
    --with-lto \
    --with-system-ffi \
    --without-ensurepip

make -j $(nproc)

make install

# python3.12.0 version requires the updated pip version. The older pip version:21.2.4 does not work with python3.12.0.
IFS='.' read -ra SPLIT_VERSION <<< "$PYTHON_VERSION"
if [ "${SPLIT_VERSION[0]}" == "3" ] && [ "${SPLIT_VERSION[1]}" -ge "12" ]
then
    export LD_LIBRARY_PATH="/opt/python/$PYTHON_VERSION/lib/"
    /opt/python/$PYTHON_VERSION/bin/python3 --version
    rm -rf /usr/src/python
    find /usr/local -depth \
        \( \
            \( -type d -a \( -name test -o -name tests -o -name idle_test \) \) \
            -o \( -type f -a \( -name '*.pyc' -o -name '*.pyo' -o -name '*.a' \) \) \
        \) -exec rm -rf '{}' + \

    ldconfig

    # make some useful symlinks that are expected to exist
    cd /opt/python/$PYTHON_VERSION/bin/
    ln -s idle3 idle
    ln -s pydoc3 pydoc
    ln -s python3 python
    ln -s python3-config python-config

    # Install pip
    wget "$PYTHON_GET_PIP_URL" -O get-pip.py

    /opt/python/$PYTHON_VERSION/bin/python3 get-pip.py \
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
if [ -n "$pipReqSetPath" ]; then
    sed -i 's|logger\.debug('\''Cleaning up\.\.\.'\'')|logger\.info('\''Cleaning up\.\.\.'\'')|' "$pipReqSetPath"
fi

compressedSdkDir="/tmp/compressedSdk/python"
mkdir -p $compressedSdkDir
cd "$INSTALLATION_PREFIX"
tar -zcf $compressedSdkDir/$pythonSdkFileName .
