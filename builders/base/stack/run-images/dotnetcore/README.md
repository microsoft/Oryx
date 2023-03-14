## Create all .NET run images

```
cd D:\oryx-builder\stack\run-images\dotnetcore
docker build . -f .\dotnetcore6.Dockerfile -t oryx/stack/dotnetcore:6.0
docker build . -f .\dotnetcore7.Dockerfile -t oryx/stack/dotnetcore:7.0
```