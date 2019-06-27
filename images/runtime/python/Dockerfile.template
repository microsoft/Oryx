# Startup script generator
FROM golang:1.11-stretch as startupCmdGen
# GOPATH is set to "/go" in the base image
WORKDIR /go/src
COPY src/startupscriptgenerator/src .
ARG GIT_COMMIT=unspecified
ARG BUILD_NUMBER=unspecified
ENV GIT_COMMIT=${GIT_COMMIT}
ENV BUILD_NUMBER=${BUILD_NUMBER}
RUN ./build.sh python /opt/startupcmdgen/startupcmdgen

FROM buildpack-deps:stretch AS main

ENV PYTHON_VERSION %PYTHON_FULL_VERSION%

# Add python binaries from Oryx base image
COPY --from=mcr.microsoft.com/oryx/python-build-base:%PYTHON_VERSION%-%IMAGE_TAG% /opt /opt
RUN set -ex \
 && cd /opt/python/ \
 && ln -s %PYTHON_FULL_VERSION% %PYTHON_VERSION% \
 && ln -s %PYTHON_VERSION% %PYTHON_MAJOR_VERSION% \
 && echo /opt/python/%PYTHON_MAJOR_VERSION%/lib >> /etc/ld.so.conf.d/python.conf \
 && ldconfig \
 && if [ "%PYTHON_MAJOR_VERSION%" = "3" ]; then cd /opt/python/%PYTHON_MAJOR_VERSION%/bin \
 && ln -s idle3 idle \
 && ln -s pydoc3 pydoc \
 && ln -s python3-config python-config; fi

ENV PATH="/opt/python/%PYTHON_MAJOR_VERSION%/bin:${PATH}"

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}
COPY images/runtime/python/install-dependencies.sh /tmp/scripts/install-dependencies.sh
RUN /tmp/scripts/install-dependencies.sh
RUN rm -rf /tmp/scripts
COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen
