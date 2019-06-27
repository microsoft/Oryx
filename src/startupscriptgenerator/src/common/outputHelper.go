// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"strings"
)

func AppendScriptToExtractZippedOutput(scriptBuilder *strings.Builder, appDir string) {
	zipFileName := "oryx_output.tar.gz"
	scriptBuilder.WriteString("\n")
	// Cannot use 'declare' here as 'sh' does not support it
	scriptBuilder.WriteString("readonly zipFileName=\"" + zipFileName + "\"\n")
	scriptBuilder.WriteString("readonly appDir=\"" + appDir + "\"\n")
	scriptBuilder.WriteString("cd \"$appDir\"\n")
	scriptBuilder.WriteString("if [ -f \"$zipFileName\" ]; then\n")
	scriptBuilder.WriteString(
		"    echo \"Found '$zipFileName' under '$appDir'. Extracting it's contents into it...\"\n")
	scriptBuilder.WriteString("    tar -xzf \"$zipFileName\"\n")
	scriptBuilder.WriteString("    echo Done.\n")
	scriptBuilder.WriteString("    echo \"Deleting the file '$zipFileName'...\"\n")
	scriptBuilder.WriteString("    rm -f \"$zipFileName\"\n")
	scriptBuilder.WriteString("    echo Done.\n")
	scriptBuilder.WriteString("fi\n\n")
}

func AppendScriptToCopyToDir(scriptBuilder *strings.Builder, srcDir string, destDir string) {
	// Cannot use 'declare' here as 'sh' does not support it
	scriptBuilder.WriteString("\n")
	scriptBuilder.WriteString("readonly srcDir=\"" + srcDir + "\"\n")
	scriptBuilder.WriteString("readonly destDir=\"" + destDir + "\"\n")
	scriptBuilder.WriteString("if [ -d \"$destDir\" ]; then\n")
	scriptBuilder.WriteString("    echo \"Directory '$destDir' already exists. Deleting it...\"\n")
	scriptBuilder.WriteString("    rm -rf \"$destDir\"\n")
	scriptBuilder.WriteString("    echo Done.\n")
	scriptBuilder.WriteString("fi\n")
	scriptBuilder.WriteString("mkdir -p \"$destDir\"\n")
	scriptBuilder.WriteString("echo \"Copying content from '$srcDir' to directory '$destDir'...\"\n")
	scriptBuilder.WriteString("cp -rf \"$srcDir\"/* \"$destDir\"\n")
	scriptBuilder.WriteString("echo Done.\n")
}
