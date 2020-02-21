# Startup script generator
FROM golang:1.11-stretch as startupCmdGen
# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ARG RELEASE_TAG_NAME=unspecified
ENV RELEASE_TAG_NAME=${RELEASE_TAG_NAME}
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
RUN ./build.sh php /opt/startupcmdgen/startupcmdgen

FROM %RUNTIME_BASE_IMAGE_NAME%

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
RUN ln -s /opt/startupcmdgen/startupcmdgen /usr/local/bin/oryx
RUN rm -rf /tmp/oryx

# Temporarily configuring and installing gd in php images, need to move to php base image building
#RUN set -eux; \
#    if [[ $PHP_VERSION == 7.4.* ]]; then \
#		docker-php-ext-configure gd --with-freetype --with-jpeg \
#        && PHP_OPENSSL=yes docker-php-ext-configure imap --with-kerberos --with-imap-ssl ; \
#    else \
#		docker-php-ext-configure gd --with-png-dir=/usr --with-jpeg-dir=/usr \
#        && docker-php-ext-configure imap --with-kerberos --with-imap-ssl ; \
#    fi \
#    ; \
#    docker-php-ext-install gd
