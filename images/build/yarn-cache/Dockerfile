# This image contains a cache with common yarn packages.
# It is never used directly, but rather as a base for our build image.
# Yarn cache will be placed in /usr/local/share/yarn-cache, that is the folder
# that must be copied to the build image.
FROM node:lts
COPY node-cache-packages.sh /tmp/
RUN chmod +x /tmp/node-cache-packages.sh \ 
 && /tmp/node-cache-packages.sh \
 && rm /tmp/node-cache-packages.sh