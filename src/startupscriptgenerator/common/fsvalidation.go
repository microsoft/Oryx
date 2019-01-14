// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"fmt"
	"io/ioutil"
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

// Writes the entrypoint command to an executable file
func WriteScript(filePath string, command string) {
	fmt.Println("Writing output script to '" + filePath + "'")
	ioutil.WriteFile(filePath, []byte(command), 0755)
}
