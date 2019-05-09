docker run -v /var/run/docker.sock:/var/run/docker.sock -v ~/Src/ACR/sajayantony-buildpacks/node-app:/mnt/app oryxdevms/pack:latest build app --path /mnt/app --no-pull
docker run app
docker rmi app