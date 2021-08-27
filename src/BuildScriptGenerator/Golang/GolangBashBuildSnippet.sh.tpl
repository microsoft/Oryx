# This will build go build applications
# Requirements:
#	./go.mod
#	./main.go

echo "   "
echo "Using Go version: "
go version
echo "   " 
echo "   "

# TODO: add time metrics
# TODO: look into go tidy
#		which removed unused dependencies
#		look into go vendor, which caches dependencies

# TODO: add support for nested dirs
echo "building go app..."
go build

echo "list of module dependencies"
go list -m