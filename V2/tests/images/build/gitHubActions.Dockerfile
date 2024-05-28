ARG PARENT_IMAGE_BASE
FROM oryxdevmcr.azurecr.io/public/oryx/build:${PARENT_IMAGE_BASE}

# Following is a pattern that AppService currently uses
RUN groupadd -g 1002 oryx_group
RUN useradd -u 1001 -g oryx_group oryx_user
RUN chown -R oryx_user:oryx_group /tmp

# Grant permissions to user's home folder as languages like .NET Core and Node
# try using it.
RUN mkdir -p /home/oryx_user
RUN chmod -R 777 /home/oryx_user

# Run the container as the following user
USER oryx_user