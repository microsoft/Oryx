// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"strings"
)

// Order of precedence for value of settings: Explicit command like argument => Environment Variable
func SetEnvironmentVariableInScript(scriptBuilder *strings.Builder, environmentVariableName string, explicitValue string, defaultValue string){
	if explicitValue != "" {
		scriptBuilder.WriteString("export " + environmentVariableName + "=" + explicitValue + "\n\n")
	} else {
		scriptBuilder.WriteString("if [ -z \"$" + environmentVariableName + "\" ]; then" + "\n");
		scriptBuilder.WriteString("		export " + environmentVariableName + "=" + defaultValue + "\n")
		scriptBuilder.WriteString("fi\n\n")
	}
}