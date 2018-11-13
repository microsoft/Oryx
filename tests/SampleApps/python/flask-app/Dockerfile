FROM oryxdevms/build as builder
WORKDIR /app
COPY . .
RUN oryx build .

FROM oryxdevms/python-3.7 as final
WORKDIR /app
COPY --from=builder /app .
RUN chmod a+x ./entryPoint.sh
EXPOSE 5000
ENTRYPOINT ["./entryPoint.sh"]