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
	SourcePath    string
	StartupCmd    string
	BindPort      string
	Manifest      common.BuildManifest
	Configuration Configuration
}

func (gen *PhpStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("php.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for source.")

	var _phpOrigin = gen.Configuration.PhpOrigin
	var portEnvVariable = "APACHE_PORT"

	if _phpOrigin == "php-fpm" {
		portEnvVariable = "NGINX_PORT"
	}

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")

	common.SetupPreRunScript(&scriptBuilder, gen.SourcePath, gen.Configuration.PreRunCommand)

	scriptBuilder.WriteString("# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.SourcePath + "\n")
	common.SetEnvironmentVariableInScript(&scriptBuilder, portEnvVariable, gen.BindPort, DefaultBindPort)
	scriptBuilder.WriteString("if [  -n \"$PHP_ORIGIN\" ] && [ \"$PHP_ORIGIN\" = \"php-fpm\" ]; then\n")
	scriptBuilder.WriteString("   export NGINX_DOCUMENT_ROOT='" + gen.SourcePath + "'\n")
	scriptBuilder.WriteString("   service nginx start\n")
	scriptBuilder.WriteString("   # Set default FPM_MAX_CHILDREN if not provided by customer or FPM_MAX_CHILDREN\n")
	scriptBuilder.WriteString("   # is not a number\n")
	scriptBuilder.WriteString("   if [ -z \"$FPM_MAX_CHILDREN\" ] || ! [ $FPM_MAX_CHILDREN -eq $FPM_MAX_CHILDREN 2> /dev/null ]; then\n")
	scriptBuilder.WriteString("      echo 'FPM_MAX_CHILDREN is not a number, setting to 5'\n")
	scriptBuilder.WriteString("      FPM_MAX_CHILDREN=\"5\"\n")
	scriptBuilder.WriteString("      export FPM_MAX_CHILDREN=\"$FPM_MAX_CHILDREN\"\n")
	scriptBuilder.WriteString("   fi\n")
	scriptBuilder.WriteString("   sed -i \"s/pm.max_children = .*/pm.max_children = ${FPM_MAX_CHILDREN}/g\" /usr/local/etc/php-fpm.d/www.conf\n")
	scriptBuilder.WriteString("else\n")
	scriptBuilder.WriteString("   export APACHE_DOCUMENT_ROOT='" + gen.SourcePath + "'\n")
	scriptBuilder.WriteString("fi\n\n")

	startupCommand := gen.StartupCmd
	if startupCommand == "" {
		startupCommand = gen.getStartupCommand()
	}

	scriptBuilder.WriteString(startupCommand + "\n")

	var runScript = scriptBuilder.String()
	return runScript
}

func (gen *PhpStartupScriptGenerator) getStartupCommand() string {
	startupCommand := "apache2-foreground"
	if gen.Configuration.PhpOrigin == "php-fpm" {
		startupCommand = "php-fpm"
	}
	return startupCommand
}
