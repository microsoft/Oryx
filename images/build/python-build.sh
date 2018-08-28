#!/bin/bash
set -ex

wget https://www.python.org/ftp/python/${PYTHON_VERSION%%[a-z]*}/Python-$PYTHON_VERSION.tar.xz -O /python.tar.xz
wget https://www.python.org/ftp/python/${PYTHON_VERSION%%[a-z]*}/Python-$PYTHON_VERSION.tar.xz.asc -O /python.tar.xz.asc

## Try 5 times at most else it's fine to fail the build
for (( i=0; i<5; i++ )); do

    gpg --keyserver hkp://p80.pool.sks-keyservers.net:80 --recv-keys $GPG_KEY || \
    gpg --keyserver hkp://ipv4.pool.sks-keyservers.net --recv-keys $GPG_KEY || \
    gpg --keyserver hkp://pgp.mit.edu:80 --recv-keys $GPG_KEY  || \
    gpg --keyserver ha.pool.sks-keyservers.net --recv-keys $GPG_KEY;
    
    ##if we get sucess during execution we will break the loop
    if [ $? -eq 0 ]; then break; fi    
     
done;
    
gpg --batch --verify /python.tar.xz.asc /python.tar.xz
tar -xJf /python.tar.xz --strip-components=1 -C .

if [ "${PYTHON_VERSION::1}" == "2" ]; then
    ./configure \
        --prefix=/opt/python/$PYTHON_VERSION \
        --build=$(dpkg-architecture --query DEB_BUILD_GNU_TYPE) \
        --enable-shared \
        --enable-unicode=ucs4
else
    ./configure \
        --prefix=/opt/python/$PYTHON_VERSION \
        --build=$(dpkg-architecture --query DEB_BUILD_GNU_TYPE) \
        --enable-loadable-sqlite-extensions \
        --enable-shared \
        --with-system-expat \
        --with-system-ffi \
        --without-ensurepip
fi

make -j $(nproc)

make install

PYTHON_PATH=/opt/python/$PYTHON_VERSION

wget https://bootstrap.pypa.io/get-pip.py -O /get-pip.py
LD_LIBRARY_PATH=/usr/src/python \
/usr/src/python/python /get-pip.py \
    --prefix $PYTHON_PATH \
    --disable-pip-version-check \
    --no-cache-dir \
    --no-warn-script-location \
    pip==$PIP_VERSION

if [ "${PYTHON_VERSION::1}" == "3" ]; then
    LD_LIBRARY_PATH=$PYTHON_PATH/lib \
    $PYTHON_PATH/bin/pip install --no-cache-dir virtualenv
fi
