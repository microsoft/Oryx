// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"common/consts"
	"fmt"
	"path/filepath"
)

func AppendScriptToExtractZippedOutput(scriptBuilder *ScriptBuilder, appDir string, intermediateDir string) {
	// Since 'appDir' is the main directory containing the output try checking to see if the exists
	// under that folder first
	fullZipFilePath := filepath.Join(appDir, consts.CompressedOutputFileName)

	// if intermediate directory is present, we want to extract content while in it
	if intermediateDir == "" {
		intermediateDir = appDir
	}

	if FileExists(fullZipFilePath) {
		scriptBuilder.AppendEmptyLine()
		scriptBuilder.Echo("Extracting '" + consts.CompressedOutputFileName + "' contents...")
		scriptBuilder.ChangeDirectory(intermediateDir)
		scriptBuilder.ExtractCompressedFile(consts.CompressedOutputFileName)
		scriptBuilder.Echo("Done")
	} else {
		fmt.Printf("Could not find the compressed file '%s'\n", fullZipFilePath)
	}
}

func AppendScriptToCopyToDir(scriptBuilder *ScriptBuilder, srcDir string, destDir string) {
	scriptBuilder.AppendEmptyLine()
	scriptBuilder.AppendLine("if [ -d \"" + destDir + "\" ]; then")
	scriptBuilder.AppendLine("    echo Directory '" + destDir + "' already exists. Deleting it...")
	scriptBuilder.AppendLine("    rm -rf \"" + destDir + "\"")
	scriptBuilder.AppendLine("    echo Done.")
	scriptBuilder.AppendLine("fi")
	scriptBuilder.MakeDirectory(destDir)
	scriptBuilder.Echo("Copying content from '" + srcDir + "' to directory '" + destDir + "'...")
	scriptBuilder.CopyDirectoryCotent(srcDir, destDir)
	scriptBuilder.Echo("Done.")
}
