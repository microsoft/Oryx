FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY . ./
WORKDIR /app/
RUN dotnet publish -c Release -o published_app

FROM mcr.microsoft.com/mirror/docker/library/nginx:1.24 AS build
WORKDIR /src
RUN apt-get update && apt-get install -y git wget build-essential libssl-dev libpcre3-dev zlib1g-dev libbrotli-dev 
RUN CONFARGS=$(nginx -V 2>&1 | sed -n -e 's/^.*arguments: //p') \
    git clone https://github.com/google/ngx_brotli.git && \
    cd ngx_brotli && git submodule update --init && cd .. && \
    wget -nv http://nginx.org/download/nginx-1.24.0.tar.gz -O - | tar -xz && \
    cd nginx-1.24.0 && \ 
    ./configure --with-compat $CONFARGS --add-dynamic-module=../ngx_brotli

WORKDIR nginx-1.24.0
RUN	make modules

FROM mcr.microsoft.com/mirror/docker/library/nginx:1.24 as final

COPY --from=build /src/nginx-1.24.0/objs/ngx_http_brotli_filter_module.so /usr/lib/nginx/modules/
COPY --from=build /src/nginx-1.24.0/objs/ngx_http_brotli_static_module.so /usr/lib/nginx/modules/

WORKDIR /var/www/web
COPY --from=build-env /app/published_app/wwwroot .
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80 443