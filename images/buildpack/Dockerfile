FROM mcr.microsoft.com/oryx/build:latest

COPY images/buildpack/bin /opt/buildpack/bin
RUN ln -s /opt/buildpack /opt/buildpack@latest

WORKDIR /tmp
RUN wget https://github.com/buildpack/pack/releases/download/v0.0.9/pack-0.0.9-linux.tar.gz && \
	tar -xvf pack-0.0.9-linux.tar.gz && \
	mv pack /usr/local/bin

ENTRYPOINT ["/usr/local/bin/pack", "build", "app", "--buildpack", "/opt/buildpack", "--path", "/mnt/app"]
