# DisableDockerDetector "This is a template Dockerfile not used to produce any Oryx images"
ARG RUNTIME={{ RuntimeImageName }}:{{ RuntimeImageTag }}

FROM mcr.microsoft.com/oryx/{{ BuildImageName }}:{{ BuildImageTag }} as build
WORKDIR /app
COPY . .
RUN oryx build /app --output /output

FROM mcr.microsoft.com/oryx/${RUNTIME}
WORKDIR /app
COPY --from=build /output .
RUN oryx create-script {{ CreateScriptArguments }}
ENTRYPOINT ["/app/run.sh"]