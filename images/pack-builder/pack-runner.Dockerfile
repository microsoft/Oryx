FROM buildpack-deps:stable

WORKDIR /tmp

COPY images/pack-builder/install-pack.sh install-pack.sh
RUN ./install-pack.sh && mv pack /usr/local/bin && rm install-pack.sh

ARG BUILDPACK_BUILDER_NAME
RUN pack set-default-builder $BUILDPACK_BUILDER_NAME

ENTRYPOINT ["/usr/local/bin/pack"]
