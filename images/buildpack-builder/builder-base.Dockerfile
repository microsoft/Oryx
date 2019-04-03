FROM oryxdevms/build:latest

LABEL io.buildpacks.stack.id=com.microsoft.oryx.stack

# Configure non-root user
RUN groupadd -g 1002 oryx_group && \
	useradd -u 1001 -g oryx_group oryx_user && \
	chown -R oryx_user:oryx_group /tmp && \
	mkdir -p /home/oryx_user && \
	chmod -R 777 /home/oryx_user

ENV PACK_USER_ID=1001 PACK_GROUP_ID=1002
ENV  CNB_USER_ID=1001  CNB_GROUP_ID=1002

COPY --from=packs/samples:v3alpha2 /lifecycle /lifecycle

RUN mkdir /platform
