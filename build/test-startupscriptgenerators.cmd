set -ex
echo "Running tests in golang docker image..."
docker run -v %CD%\..\src\startupscriptgenerator\:/src:ro golang:1.11-stretch bash -c^
 "cp -rf /src /go/src/startupscriptgenerator && cd /go/src/startupscriptgenerator && ./prepare-go-env.sh && go test startupscriptgenerator/... -v"