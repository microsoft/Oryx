FROM oryxdevms/build as builder
WORKDIR /app
COPY . .
RUN oryx build . -l python --language-version 2.7

FROM oryxdevms/python-2.7 as final
WORKDIR /app
COPY --from=builder /app .
RUN chmod a+x ./entryPoint.sh
EXPOSE 5000
ENTRYPOINT ["./entryPoint.sh"]