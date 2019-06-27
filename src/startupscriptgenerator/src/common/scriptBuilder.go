// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"strings"
)

type ScriptBuilder struct {
	scriptBuilder strings.Builder
}

func (builder *ScriptBuilder) AppendEmptyLine() {
	builder.scriptBuilder.WriteString("\n")
}

func (builder *ScriptBuilder) AppendLine(line string) {
	builder.scriptBuilder.WriteString(line + "\n")
}

func (builder *ScriptBuilder) CopyDirectoryCotent(sourceDir string, destinationDir string) {
	builder.AppendLine("cp -rf \"" + sourceDir + "\"/* \"" + destinationDir + "\"")
}

func (builder *ScriptBuilder) MakeDirectory(directoryName string) {
	builder.AppendLine("mkdir -p \"" + directoryName + "\"")
}

func (builder *ScriptBuilder) Echo(content string) {
	builder.AppendLine("echo " + content)
}

func (builder *ScriptBuilder) ExtractCompressedFile(fileName string) {
	builder.AppendLine("tar -xzf \"" + fileName + "\"")
}

func (builder *ScriptBuilder) ChangeDirectory(directoryName string) {
	builder.AppendLine("cd \"" + directoryName + "\"")
}

func (builder *ScriptBuilder) ExportVariable(name string, value string) {
	builder.AppendLine("export " + name + "=\"" + value + "\"")
}

func (builder *ScriptBuilder) ToString() string {
	return builder.scriptBuilder.String()
}
