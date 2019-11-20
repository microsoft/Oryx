FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS buildscriptbuilder
COPY src/BuildScriptGenerator /usr/oryx/src/BuildScriptGenerator
COPY src/BuildScriptGeneratorCli /usr/oryx/src/BuildScriptGeneratorCli
COPY src/Common /usr/oryx/src/Common
COPY build/FinalPublicKey.snk usr/oryx/build/
COPY src/CommonFiles /usr/oryx/src/CommonFiles
# This statement copies signed oryx binaries from during agent build.
# For local/dev contents of blank/empty directory named binaries are getting copied
COPY binaries /opt/buildscriptgen/
WORKDIR /usr/oryx/src
ARG GIT_COMMIT=unspecified
ARG AGENTBUILD=${AGENTBUILD}
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ARG AGENTBUILD=${AGENTBUILD}
RUN if [ -z "$AGENTBUILD" ]; then \
        dotnet publish -r linux-x64 -o /opt/buildscriptgen/ -c Release BuildScriptGeneratorCli/BuildScriptGeneratorCli.csproj; \
    fi
RUN chmod a+x /opt/buildscriptgen/GenerateBuildScript

FROM buildpack-deps:stretch-curl
SHELL [ "/bin/bash", "-c" ]
ARG BASH_RC="/root/.bashrc"

RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y --no-install-recommends \
        apt-transport-https \
        xz-utils \
        git

# Install .NET Core SDKs
RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg \
    && mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/ \
    && wget -q https://packages.microsoft.com/config/debian/10/prod.list \
            -O /etc/apt/sources.list.d/microsoft-prod.list

#RUN apt-get update \
#    && apt-get install -y --no-install-recommends \
#        dotnet-sdk-2.1 \
#        dotnet-sdk-2.2 \
#        dotnet-sdk-3.0 \
#        dotnet-sdk-3.1

# Install nvm
ENV NVM_DIR /usr/local/nvm
RUN mkdir -p $NVM_DIR
RUN curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.35.1/install.sh | bash

#RUN source $NVM_DIR/nvm.sh \
#    && nvm install 8 \
#    && nvm install 10 \
#    && nvm install 12

# Install pyenv (python's version manager)
# Install python pre-reqs
RUN apt-get install -y --no-install-recommends \
        make \
        build-essential \
        libssl-dev \
        zlib1g-dev \
        libbz2-dev \
        libreadline-dev \
        libsqlite3-dev \
        wget \
        curl \
        llvm \
        libncurses5-dev \
        xz-utils \
        tk-dev \
        libxml2-dev \
        libxmlsec1-dev \
        libffi-dev \
        liblzma-dev

ENV PYENV_ROOT="/usr/local/pyenv"
RUN mkdir $PYENV_ROOT
RUN git clone --recursive --shallow-submodules \
        https://github.com/pyenv/pyenv.git \
        $PYENV_ROOT

RUN echo "export PYENV_ROOT=$PYENV_ROOT" >> $BASH_RC
RUN echo "export PYENV_ROOT=$PYENV_ROOT" >> $BASH_RC
RUN echo 'export PATH=$PYENV_ROOT/bin:$PATH' >> $BASH_RC
RUN echo -e 'if command -v pyenv 1>/dev/null 2>&1; then\n  eval "$(pyenv init -)"\nfi' >> $BASH_RC

#RUN $PYENV_ROOT/bin/pyenv install 2.7.17 \
#    && $PYENV_ROOT/bin/pyenv install 3.7.5 \
#    && $PYENV_ROOT/bin/pyenv install 3.8.0

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        build-essential \
        libbz2-dev \
        libreadline-dev \
        libsqlite3-dev \
        libssl-dev \
        libxml2-dev \
        libxslt1-dev \
        php7.0-cli \
        pkg-config \
        zip \
        libzip-dev

RUN curl -L -O https://github.com/phpbrew/phpbrew/raw/master/phpbrew \
    && chmod +x phpbrew \
    && mv phpbrew /usr/local/bin/phpbrew

RUN phpbrew init \
    && echo '[[ -e ~/.phpbrew/bashrc ]] && source ~/.phpbrew/bashrc' >> $BASH_RC

# Workaround as indicated here https://github.com/phpbrew/phpbrew/issues/861#issuecomment-310196141
RUN cd /usr/local/include \
    && ln -s /usr/include/x86_64-linux-gnu/curl curl \
    && apt-get install -y \
        libcurl4-gnutls-dev \
        libonig-dev

# Install composer
RUN phpbrew app get composer
RUN echo 'export PATH="$PATH:$HOME/.phpbrew/bin"' >> $BASH_RC

#RUN phpbrew install 7.3
#RUN phpbrew install 7.4

RUN mkdir -p /op/oryx
COPY images/build/dev.benv.sh /opt/oryx/benv
RUN chmod +x /opt/oryx/benv
ENV PATH="$PATH:/opt/oryx"

# Build script generator content. Docker doesn't support variables in --from
# so we are building an extra stage to copy binaries from correct build stage
COPY --from=buildscriptbuilder /opt/buildscriptgen/ /opt/buildscriptgen/
RUN ln -s /opt/buildscriptgen/GenerateBuildScript /opt/oryx/oryx

RUN apt-get install -y \
    rsync \
    moreutils