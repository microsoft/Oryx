# This image contains a cache with common yarn packages.
# It is never used directly, but rather as a base for our build image.
# Yarn cache will be placed in /usr/local/share/yarn-cache, that is the folder
# that must be copied to the build image.
FROM node:lts
COPY cacheNodePackages.sh /tmp/
RUN chmod +x /tmp/cacheNodePackages.sh
RUN /tmp/cacheNodePackages.sh
RUN rm /tmp/cacheNodePackages.sh