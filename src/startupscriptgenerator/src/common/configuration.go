// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

// Order of precedence for value of settings: Explicit command like argument => Environment Variable
func SetEnvironmentVariableInScript(
	scriptBuilder *ScriptBuilder,
	environmentVariableName string,
	explicitValue string,
	defaultValue string) {

	if explicitValue != "" {
		scriptBuilder.ExportVariable(environmentVariableName, explicitValue)
	} else {
		scriptBuilder.AppendLine("if [ -z \"$" + environmentVariableName + "\" ]; then")
		scriptBuilder.AppendLine("		export " + environmentVariableName + "=" + defaultValue)
		scriptBuilder.AppendLine("fi")
	}
	scriptBuilder.AppendEmptyLine()
}
