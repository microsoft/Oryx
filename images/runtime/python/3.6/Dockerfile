# Startup script generator
FROM golang:1.11-alpine as startupCmdGen
WORKDIR /go/src/startupscriptgenerator/
COPY src/startupscriptgenerator .
RUN ./build.sh python /opt/startupcmdgen/startupcmdgen

FROM python:3.6.7-slim-stretch
ADD /images/runtime/python/scripts /tmp/scripts
RUN /tmp/scripts/install-dependencies.sh
RUN rm -rf /tmp/scripts
COPY --from=startupCmdGen /opt/startupcmdgen/startupcmdgen /opt/startupcmdgen/startupcmdgen