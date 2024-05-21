FROM oryxdevmcr.azurecr.io/private/oryx/python-build-prereqs
WORKDIR /usr/src/python
ARG VERSION_TO_BUILD
ARG GPG_KEYS
ARG PIP_VERSION
RUN PYTHON_VERSION=$VERSION_TO_BUILD \
    GPG_KEY=$GPG_KEYS \
    PIP_VERSION=$PIP_VERSION \
    /tmp/build.sh
