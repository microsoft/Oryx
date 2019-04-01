# FROM golang:1.11-stretch as buildpack
# ARG BUILD_NUMBER
# # Package buildpack
# COPY images/buildpack buildpack
# RUN go get github.com/cloudfoundry/libbuildpack/packager/buildpack-packager && \
# 	go install github.com/cloudfoundry/libbuildpack/packager/buildpack-packager
# RUN cd buildpack && echo Version: ${BUILD_NUMBER} && echo ${BUILD_NUMBER} > VERSION && buildpack-packager build -any-stack

FROM mcr.microsoft.com/oryx/build:latest

# Configure non-root user
RUN groupadd -g 1002 oryx_group && \
	useradd -u 1001 -g oryx_group oryx_user && \
	chown -R oryx_user:oryx_group /tmp && \
	mkdir -p /home/oryx_user && \
	chmod -R 777 /home/oryx_user

WORKDIR /tmp

# Install pack
RUN wget -nv https://github.com/buildpack/pack/releases/download/v0.0.9/pack-0.0.9-linux.tar.gz && \
	tar -xvf pack-0.0.9-linux.tar.gz && \
	mv pack /usr/local/bin

LABEL io.buildpacks.stack.id=com.microsoft.oryx.stack

ENV PACK_STACK_ID=com.microsoft.oryx.stack
ENV  CNB_STACK_ID=com.microsoft.oryx.stack

ENV PACK_USER_ID=1001 PACK_GROUP_ID=1002
ENV  CNB_USER_ID=1001  CNB_GROUP_ID=1002

RUN pack add-stack com.microsoft.oryx.stack -b mcr.microsoft.com/oryx/build -r mcr.microsoft.com/oryx/build && \
	pack set-default-stack com.microsoft.oryx.stack && \
	pack delete-stack io.buildpacks.stacks.bionic

COPY images/buildpack/builder.toml builder.toml
COPY images/buildpack/oryx oryx
VOLUME /var/run/docker.sock /var/run/docker.sock
RUN pack create-builder oryxdevms/buildpack-builder --builder-config builder.toml
