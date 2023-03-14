FROM mcr.microsoft.com/oryx/dotnetcore:7.0

# 2. Set required CNB information
ENV CNB_USER_ID=1000
ENV CNB_GROUP_ID=1000
ENV CNB_STACK_ID="oryx.stacks.skeleton"
LABEL io.buildpacks.stack.id="oryx.stacks.skeleton"

RUN groupadd cnb --gid ${CNB_GROUP_ID} && \
  useradd --uid ${CNB_USER_ID} --gid ${CNB_GROUP_ID} -m -s /bin/bash cnb

USER ${CNB_USER_ID}:${CNB_GROUP_ID}