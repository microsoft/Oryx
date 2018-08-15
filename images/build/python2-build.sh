#!/bin/bash
set -ex

./configure \
    --prefix=/opt/python/$PYTHON_VERSION \
    --build=$(dpkg-architecture --query DEB_BUILD_GNU_TYPE) \
    --enable-shared \
    --enable-unicode=ucs4

make -j $(nproc)
