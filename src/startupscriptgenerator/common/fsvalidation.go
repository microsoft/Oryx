package fsvalidation

import (
	"os"
	"path/filepath"
)

// Gets the full path from a relative path, and ensure the path exists.
func GetValidatedFullPath(filePath string) string {
	fullAppPath, err := filepath.Abs(filePath)
	if err != nil {
		panic(err)
	}

	if _, err := os.Stat(fullAppPath); os.IsNotExist(err) {
		panic("Path '" + fullAppPath + "' does not exist.")
	}
	return fullAppPath
}
