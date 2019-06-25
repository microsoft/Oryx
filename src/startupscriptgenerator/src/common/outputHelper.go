// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"common/consts"
	"fmt"
	"path/filepath"
	"strings"
)

func AppendScriptToExtractZippedOutput(scriptBuilder *strings.Builder, appDir string, intermediateDir string) {
	// Since 'appDir' is the main directory containing the output try checking to see if the exists
	// under that folder first
	fullZipFilePath := filepath.Join(appDir, consts.CompressedOutputFileName)

	// if intermediate directory is present, we want to extract content while in it
	if intermediateDir == "" {
		intermediateDir = appDir
	}

	if FileExists(fullZipFilePath) {
		scriptBuilder.WriteString("echo Extracting '" + consts.CompressedOutputFileName + "' contents...\n")
		scriptBuilder.WriteString("cd \"" + intermediateDir + "\"\n")
		scriptBuilder.WriteString("tar -xzf \"" + consts.CompressedOutputFileName + "\"\n")
		scriptBuilder.WriteString("echo Done.\n")
		scriptBuilder.WriteString("echo Deleting the file '" + consts.CompressedOutputFileName + "'...\n")
		scriptBuilder.WriteString("rm -f \"" + consts.CompressedOutputFileName + "\"\n")
		scriptBuilder.WriteString("echo Done.\n")
	} else {
		fmt.Printf("Could not find the compressed file '%s'\n", fullZipFilePath)
	}
}

func AppendScriptToCopyToDir(scriptBuilder *strings.Builder, srcDir string, destDir string) {
	if PathExists(destDir) {
		scriptBuilder.WriteString("echo Directory '" + destDir + "' already exists. Deleting it...\n")
		scriptBuilder.WriteString("rm -rf \"" + destDir + "\"\n")
		scriptBuilder.WriteString("echo Done.\n")
	}

	scriptBuilder.WriteString("mkdir -p \"" + destDir + "\"\n")
	scriptBuilder.WriteString("echo Copying content from '" + srcDir + "' to directory '" + destDir + "'...\n")
	scriptBuilder.WriteString("cp -rf \"" + srcDir + "\"/* \"" + destDir + "\"\n")
	scriptBuilder.WriteString("echo Done.\n")
}
