#!/bin/bash
set -ex

wget https://www.python.org/ftp/python/${PYTHON_VERSION%%[a-z]*}/Python-$PYTHON_VERSION.tar.xz -O /python.tar.xz
wget https://www.python.org/ftp/python/${PYTHON_VERSION%%[a-z]*}/Python-$PYTHON_VERSION.tar.xz.asc -O /python.tar.xz.asc

## Try 5 times at most else it's fine to fail the build
for (( i=0; i<5; i++ )); do

    gpg --batch --keyserver hkp://p80.pool.sks-keyservers.net:80 --recv-keys $GPG_KEY || \
    gpg --batch --keyserver hkp://ipv4.pool.sks-keyservers.net --recv-keys $GPG_KEY || \
    gpg --batch --keyserver hkp://pgp.mit.edu:80 --recv-keys $GPG_KEY  || \
    gpg --batch --keyserver ha.pool.sks-keyservers.net --recv-keys $GPG_KEY;
    
    ##if we get sucess during execution we will break the loop
    if [ $? -eq 0 ]; then break; fi    
     
done;
    
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

# Install pip
wget https://bootstrap.pypa.io/get-pip.py -O /get-pip.py
LD_LIBRARY_PATH=/usr/src/python \
/usr/src/python/python /get-pip.py \
    --prefix $INSTALLATION_PREFIX \
    --disable-pip-version-check \
    --no-cache-dir \
    --no-warn-script-location \
    pip==$PIP_VERSION

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

# Replace log level in pip's code as a workaround for https://github.com/pypa/pip/issues/6189
pipReqSetPath=`find $INSTALLATION_PREFIX/lib -path "*site-packages/pip/_internal/req/req_set.py"`
sed -i 's|logger\.debug('\''Cleaning up\.\.\.'\'')|logger\.info('\''Cleaning up\.\.\.'\'')|' "$pipReqSetPath"
