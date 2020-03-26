// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"strings"
	"common/consts"
)

// Run pre-run script from root of the app directory
func SetupPreRunScript(scriptBuilder *strings.Builder) {
	preRunCommandOrScript := GetEnvironmentVariable(consts.PreRunCommandEnvVarName)
	if preRunCommandOrScript != "" {
		scriptBuilder.WriteString("echo 'Running the user provided pre-run command... ")
		scriptBuilder.WriteString(preRunCommandOrScript + "\n")
	}
}
