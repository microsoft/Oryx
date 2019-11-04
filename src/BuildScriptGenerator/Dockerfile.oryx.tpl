ARG RUNTIME={{ RuntimeImageName }}:{{ RuntimeImageTag }}

FROM mcr.microsoft.com/oryx/build:{{ BuildImageTag }} as build
WORKDIR /app
COPY . .
RUN oryx build /app

FROM mcr.microsoft.com/oryx/${RUNTIME}
COPY --from=build /app /app
RUN cd /app && oryx
ENTRYPOINT ["/app/run.sh"]