FROM python:2.7.15-slim-stretch

ADD /images/runtime/python/scripts /tmp/scripts
RUN chmod a+x /tmp/scripts/installDrivers.sh
RUN /tmp/scripts/installDrivers.sh
RUN rm -rf /tmp/scripts