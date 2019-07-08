FROM buildpack-deps:stretch

WORKDIR /tmp

COPY images/pack-builder/installPack.sh installPack.sh
RUN ./installPack.sh && mv pack /usr/local/bin && rm installPack.sh

ARG DEFAULT_BUILDER_NAME
RUN pack set-default-builder $DEFAULT_BUILDER_NAME

ENTRYPOINT ["/usr/local/bin/pack"]
