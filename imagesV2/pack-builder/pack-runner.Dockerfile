# DisableDockerDetector "Below image not yet supported in the Docker Hub mirror"
FROM buildpack-deps:stretch

WORKDIR /tmp

COPY imagesV2/pack-builder/installPack.sh installPack.sh
RUN ./installPack.sh && mv pack /usr/local/bin && rm installPack.sh

ENTRYPOINT ["/usr/local/bin/pack"]
