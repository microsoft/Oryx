# Startup script generator
FROM golang:1.11-alpine as startupCmdGen
COPY src/StartupScriptGenerator-Node /src/StartupScriptGenerator
WORKDIR /src/StartupScriptGenerator
RUN go build -o /opt/StartupScriptGenerator/StartupScriptGenerator . 

FROM node:10.13.0-alpine
COPY --from=startupCmdGen /opt/StartupScriptGenerator /opt/StartupScriptGenerator