// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"strings"
)

const DefaultBindPort = "8080"

type PhpStartupScriptGenerator struct {
	SourcePath string
	StartupCmd string
	BindPort   string
	Manifest   common.BuildManifest
}

func (gen *PhpStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("php.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for source at '%s'", gen.SourcePath)

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.SourcePath + "\n")
	scriptBuilder.WriteString("export APACHE_DOCUMENT_ROOT='" + gen.SourcePath + "'\n")
	common.SetEnvironmentVariableInScript(&scriptBuilder, "APACHE_PORT", gen.BindPort, DefaultBindPort)
	scriptBuilder.WriteString(gen.StartupCmd + "\n")

	logger.LogProperties("Finalizing script", map[string]string{"root": gen.SourcePath, "cmd": gen.StartupCmd})
	var runScript = scriptBuilder.String()
	logger.LogInformation("Run script content:\n" + runScript)
	return runScript
}
