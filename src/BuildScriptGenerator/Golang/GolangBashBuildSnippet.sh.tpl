echo "   "
echo "Using Go version: "
go version
echo "   " 
echo "   "


# TODO: add time metrics


# TODO: look into go tidy
#		which removed unused dependencies
#		look into go vendor, which caches dependencies

echo "building the go application"
go build

echo "list of module dependencies"
go list -m

# go install: moves binary into /bin/
go install

# run executable
ls
parentDir=$(basename $(dirname "$PWD"))
./${parentDir}

# TODO: compare with go run