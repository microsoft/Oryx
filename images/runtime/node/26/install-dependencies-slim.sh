#!/bin/bash
# --------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT license.
# --------------------------------------------------------------------------------------------

set -ex

apt-get update \
    && apt-get install -y --no-install-recommends \
        ca-certificates \
        curl \
        wget \
        less \
        git \
        gnupg \
        tzdata \
        xz-utils \
        libstdc++6 \
        libgcc-s1

# Clean up apt lists — keep at the very end so every apt-get install above
# is included.
rm -rf /var/lib/apt/lists/*
