FROM python:2.7.15-slim-stretch

ADD /images/runtime/python/scripts /tmp/scripts
RUN /tmp/scripts/installDependencies.sh
RUN rm -rf /tmp/scripts