# Startup script generator
FROM mcr.microsoft.com/oss/go/microsoft/golang:1.20-buster as startupCmdGen

# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
#Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"
RUN chmod +x build.sh && ./build.sh dotnetcore /opt/startupcmdgen/startupcmdgen


FROM mcr.microsoft.com/mirror/docker/library/debian:buster-slim
ARG BUILD_DIR=/tmp/oryx/build
ADD build ${BUILD_DIR}

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        # .NET Core dependencies
        libc6 \
        libgcc1 \
        libgssapi-krb5-2 \
        libicu63 \
        libssl1.1 \
        libstdc++6 \
        zlib1g \
        lldb \
        curl \
        file \
        libgdiplus \
    && apt-get upgrade --assume-yes \
    && rm -rf /var/lib/apt/lists/*

# Configure web servers to bind to port 80 when present
ENV ASPNETCORE_URLS=http://+:80 \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true \
    PATH="/opt/dotnetcore-tools:${PATH}"

# Bake Application Insights key from pipeline variable into final image
ARG AI_CONNECTION_STRING
ARG USER_DOTNET_AI_VERSION
ENV USER_DOTNET_AI_VERSION=${USER_DOTNET_AI_VERSION}
ENV ORYX_AI_CONNECTION_STRING=${AI_CONNECTION_STRING} 
ENV DOTNET_VERSION="6.0"
ENV ASPNETCORE_LOGGING__CONSOLE__DISABLECOLORS=true
#Bake in client certificate path into image to avoid downloading it
ENV PATH_CA_CERTIFICATE="/etc/ssl/certs/ca-certificate.crt"

ENV LANG="C.UTF-8" \
    LANGUAGE="C.UTF-8" \
    LC_ALL="C.UTF-8"

# Oryx++ Builder variables
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
COPY DotNetCoreAgent.${USER_DOTNET_AI_VERSION}.zip /DotNetCoreAgent/appinsights.zip
RUN set -e \
    && echo $USER_DOTNET_AI_VERSION \ 
    && ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx \
    && apt-get update \
    && apt-get install unzip -y \ 
    && apt-get upgrade --assume-yes \
    && cd DotNetCoreAgent \
    && unzip appinsights.zip && rm appinsights.zip

LABEL maintainer="Azure App Services Container Images <appsvc-images@microsoft.com>"

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        apt-utils \
        unzip \
        openssh-server \
        vim \
        curl \
        wget \
        tcptraceroute \
        net-tools \
        dnsutils \
        tcpdump \
        iproute2 \
        nano \
        cron \
    && apt-get upgrade -y openssl \
    && rm -rf /var/lib/apt/lists/*

COPY tcpping /usr/bin/tcpping
RUN chmod 755 /usr/bin/tcpping

RUN curl -L --insecure https://aka.ms/downloadazcopy-v10-linux | tar -C /usr/local/bin -xzf - --strip-components=1

RUN mkdir -p /defaulthome/hostingstart \
    && mkdir -p /home/LogFiles/ /opt/startup/ \
    && echo "root:Docker!" | chpasswd \
    && echo "cd /home" >> /root/.bashrc

COPY init_container.sh /bin/
RUN chmod 755 /bin/init_container.sh

# not adding chmod for this file as we're just using it as `source`.
COPY install_vsdbg.sh /bin/

COPY generate_and_execute_startup_script.sh /bin/
RUN chmod 755 /bin/generate_and_execute_startup_script.sh

COPY hostingstart.html /defaulthome/hostingstart/wwwroot/

# when openssh-server is installed, it creates set of private keys in /etc/ssh folder
# Removing these to ensure the none of the layers have private keys.
RUN rm -rf /etc/ssh/
COPY sshd_config /etc/ssh/

# configure startup
COPY ssh_setup.sh startssh.sh install_ca_certs.sh /opt/startup/
RUN chmod -R +x /opt/startup

COPY dotnet_monitor_config.json /dotnet_monitor_config.json
COPY run-dotnet-monitor.sh /run-dotnet-monitor.sh
RUN chmod +x /run-dotnet-monitor.sh

COPY run-diag.sh /run-diag.sh
RUN chmod +x /run-diag.sh

ENV PORT 8080
ENV SSH_PORT 2222
EXPOSE 8080 2222 50050

ENV WEBSITE_ROLE_INSTANCE_ID localRoleInstance
ENV WEBSITE_INSTANCE_ID localInstance
ENV PATH ${PATH}:/home/site/wwwroot
ENV ASPNETCORE_URLS=
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
ENV HOME /home

WORKDIR /home/site/wwwroot

ENTRYPOINT ["/bin/init_container.sh"]