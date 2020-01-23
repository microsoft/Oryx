// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"os"
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

	logger.LogInformation("Generating script for source.")

	var _phpOrigin = os.Getenv(consts.PhpOriginEnvVarName)
	var portEnvVariable = "APACHE_PORT"

	if _phpOrigin == "php-fpm" {
		portEnvVariable = "NGINX_PORT"
	}

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.SourcePath + "\n")
	common.SetEnvironmentVariableInScript(&scriptBuilder, portEnvVariable, gen.BindPort, DefaultBindPort)
	scriptBuilder.WriteString("if [ -n '$PHP_ORIGIN' ]; then\n")
	scriptBuilder.WriteString("   export NGINX_DOCUMENT_ROOT='" + gen.SourcePath + "'\n")
	scriptBuilder.WriteString("   service nginx start\n")
	scriptBuilder.WriteString("else\n")
	scriptBuilder.WriteString("   export APACHE_DOCUMENT_ROOT='" + gen.SourcePath + "'\n")
	scriptBuilder.WriteString("fi\n\n")

	scriptBuilder.WriteString(gen.StartupCmd + "\n")

	var runScript = scriptBuilder.String()
	return runScript
}
