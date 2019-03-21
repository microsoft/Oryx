# Build Python 3.6
FROM oryxdevmcr.azurecr.io/python-build-prereqs
WORKDIR /usr/src/python
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
RUN . /tmp/__python-versions.sh && PYTHON_VERSION=$PYTHON36_VERSION PIP_VERSION=$PIP_VERSION /tmp/build.sh
