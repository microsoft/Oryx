ARG RUNTIME={{ RuntimeImageName }}:{{ RuntimeImageTag }}

FROM mcr.microsoft.com/oryx/build:{{ BuildImageTag }} as build
WORKDIR /app
COPY . .
RUN oryx build --output /output /app

FROM mcr.microsoft.com/oryx/${RUNTIME}
WORKDIR /app
COPY --from=build /output .
RUN oryx create-script
ENTRYPOINT ["/app/run.sh"]