FROM buildpack-deps:stable

# Configure non-root user
# RUN groupadd -g 1002 oryx_group && \
# 	useradd -u 1001 -g oryx_group oryx_user && \
# 	chown -R oryx_user:oryx_group /tmp && \
# 	mkdir -p /home/oryx_user && \
# 	chmod -R 777 /home/oryx_user

WORKDIR /tmp

COPY images/buildpack-builder/install-pack.sh install-pack.sh
RUN ./install-pack.sh && mv pack /usr/local/bin

ARG BUILDPACK_BUILDER_NAME
RUN pack set-default-builder $BUILDPACK_BUILDER_NAME

ENTRYPOINT ["/usr/local/bin/pack"]
