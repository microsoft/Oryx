// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"fmt"
	"path/filepath"
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

	if gen.Manifest.CompressDestinationDir == "true" {
		println("Output is compressed. Extracting it...")
		tarballFile := filepath.Join(gen.SourcePath, "output.tar.gz")
		common.ExtractTarball(tarballFile, gen.Manifest.SourceDirectoryInBuildContainer)
		println(fmt.Sprintf("App path is set to '%s'", gen.Manifest.SourceDirectoryInBuildContainer))
	}

	scriptBuilder.WriteString(fmt.Sprintf("echo 'export APP_PATH=\"%s\"' >> ~/.bashrc\n", gen.getAppPath()))
	scriptBuilder.WriteString("echo 'cd $APP_PATH' >> ~/.bashrc\n")

	common.SetupPreRunScript(&scriptBuilder, gen.getAppPath(), gen.Configuration.PreRunCommand)

	scriptBuilder.WriteString("# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.getAppPath() + "\n")
	common.SetEnvironmentVariableInScript(&scriptBuilder, portEnvVariable, gen.BindPort, DefaultBindPort)
	scriptBuilder.WriteString("if [  -n \"$PHP_ORIGIN\" ] && [ \"$PHP_ORIGIN\" = \"php-fpm\" ]; then\n")
	scriptBuilder.WriteString("   export NGINX_DOCUMENT_ROOT='" + gen.getAppPath() + "'\n")
	scriptBuilder.WriteString("   service nginx start\n")
	scriptBuilder.WriteString("else\n")
	scriptBuilder.WriteString("   export APACHE_DOCUMENT_ROOT='" + gen.getAppPath() + "'\n")
	scriptBuilder.WriteString("fi\n\n")

	startupCommand := gen.StartupCmd
	if startupCommand == "" {
		startupCommand = gen.getStartupCommand()
	}

	scriptBuilder.WriteString(startupCommand + "\n")

	var runScript = scriptBuilder.String()
	return runScript
}

func (gen *PhpStartupScriptGenerator) getAppPath() string {
	if gen.Manifest.CompressDestinationDir == "true" && gen.Manifest.SourceDirectoryInBuildContainer != "" {
		return gen.Manifest.SourceDirectoryInBuildContainer
	}

	return gen.SourcePath
}

func (gen *PhpStartupScriptGenerator) getStartupCommand() string {
	startupCommand := "apache2-foreground"
	if gen.Configuration.PhpOrigin == "php-fpm" {
		startupCommand = "php-fpm"
	}
	return startupCommand
}
