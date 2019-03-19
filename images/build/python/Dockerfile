# Install Python build prerequisites
FROM buildpack-deps:stable AS python-build-prereqs
COPY build/__python-versions.sh /tmp/
COPY images/build/python/build.sh /
RUN chmod +x /build.sh
RUN apt-get update && \
	apt-get install -y --no-install-recommends \
		tk-dev \
		uuid-dev

# Build Python 2.7
FROM python-build-prereqs AS python27-build
WORKDIR /usr/src/python
ENV GPG_KEY C01E1CAD5EA2C4F0B8E3571504C367C218ADD4FF
RUN . /tmp/__python-versions.sh && PYTHON_VERSION=$PYTHON27_VERSION PIP_VERSION=$PIP_VERSION /build.sh

# Build Python 3.5
FROM python-build-prereqs AS python35-build
WORKDIR /usr/src/python
ENV GPG_KEY 97FC712E4C024BBEA48A61ED3A5CA953F73C700D
RUN . /tmp/__python-versions.sh && PYTHON_VERSION=$PYTHON35_VERSION PIP_VERSION=$PIP_VERSION /build.sh

# Build Python 3.6
FROM python-build-prereqs AS python36-build
WORKDIR /usr/src/python
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
RUN . /tmp/__python-versions.sh && PYTHON_VERSION=$PYTHON36_VERSION PIP_VERSION=$PIP_VERSION /build.sh

# Build Python 3.7
FROM python-build-prereqs AS python37-build
WORKDIR /usr/src/python
ENV GPG_KEY=0D96DF4D4110E5C43FBFB17F2D347EA6AA65421D
RUN . /tmp/__python-versions.sh && PYTHON_VERSION=$PYTHON37_VERSION PIP_VERSION=$PIP_VERSION /build.sh
