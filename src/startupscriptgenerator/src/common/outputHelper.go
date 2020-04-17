// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"strings"
)

func AppendScriptToCopyToDir(scriptBuilder *strings.Builder, srcDir string, destDir string) {
	scriptBuilder.WriteString("\n")
	scriptBuilder.WriteString("declare -r srcDir=\"" + srcDir + "\"\n")
	scriptBuilder.WriteString("declare -r destDir=\"" + destDir + "\"\n")
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
