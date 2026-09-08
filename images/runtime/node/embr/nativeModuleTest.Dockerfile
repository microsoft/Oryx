# syntax=docker/dockerfile:1.7

ARG NODE_MAJOR
ARG RUNTIME_IMAGE

FROM node:${NODE_MAJOR}-bookworm AS nativebuild

WORKDIR /app
RUN npm init --yes \
    && npm install --omit=dev --save-exact sharp@0.34.5

FROM ${RUNTIME_IMAGE}

COPY --from=nativebuild /app /tmp/native-module-test
RUN node -e \
    'const sharp = require("/tmp/native-module-test/node_modules/sharp"); sharp({ create: { width: 1, height: 1, channels: 4, background: "red" } }).png().toBuffer().then(output => { if (output.length === 0) process.exit(1); })'
