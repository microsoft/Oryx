# Startup script generator
FROM golang:1.11-stretch as startupCmdGen
WORKDIR /go/src/startupscriptgenerator/
COPY src/startupscriptgenerator .
RUN ./build.sh python /opt/startupcmdgen/startupcmdgen

FROM python:3.7.1-slim-stretch

# Bake Application Insights key from pipeline variable into final image
ARG AI_KEY
ENV ORYX_AI_INSTRUMENTATION_KEY=${AI_KEY}

ADD /images/runtime/python/scripts /tmp/scripts
RUN /tmp/scripts/install-dependencies.sh
RUN rm -rf /tmp/scripts
COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen