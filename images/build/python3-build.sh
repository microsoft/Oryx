#!/bin/bash
set -ex

./configure \
    --prefix=/opt/python/$PYTHON_VERSION \
    --build=$(dpkg-architecture --query DEB_BUILD_GNU_TYPE) \
    --enable-loadable-sqlite-extensions \
    --enable-shared \
    --with-system-expat \
	--with-system-ffi \
	--without-ensurepip

make -j $(nproc)
