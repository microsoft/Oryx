#!/bin/bash
set -ex

wget https://www.openssl.org/source/openssl-1.1.1.tar.gz -O /openssl.tar.gz
tar -xzf /openssl.tar.gz --strip-components=1 -C .

./config

make -j $(nproc)

make install

ldconfig