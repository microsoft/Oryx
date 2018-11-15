FROM python:3.7.1-slim-stretch

ADD /images/runtime/python/scripts /tmp/scripts
RUN /tmp/scripts/installDependencies.sh
RUN rm -rf /tmp/scripts
