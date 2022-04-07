// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"strings"
	"fmt"
)

type GolangStartupScriptGenerator struct {
	AppPath                  string
	UserStartupCommand       string
	DefaultAppPath           string
	BindPort                 string
	Manifest                 common.BuildManifest
	Configuration            Configuration
}

const DefaultHost = "0.0.0.0"
const DefaultBindPort = "80"

func (gen *GolangStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("golang.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n\n")

	command := gen.UserStartupCommand // A custom command takes precedence over any framework defaults
	if command != "" {
		isPermissionAdded := common.ParseCommandAndAddExecutionPermission(gen.UserStartupCommand, gen.AppPath)
		logger.LogInformation("Permission added: %t", isPermissionAdded)
		command = common.ExtendPathForCommand(command, gen.AppPath)
	}

	// set APP_PATH
	scriptBuilder.WriteString(fmt.Sprintf("echo 'export APP_PATH=\"%s\"' >> ~/.bashrc\n", gen.AppPath))
	scriptBuilder.WriteString("echo 'cd $APP_PATH' >> ~/.bashrc\n")
	common.SetupPreRunScript(&scriptBuilder, gen.getAppPath(), gen.Configuration.PreRunCommand)
	scriptBuilder.WriteString("\n# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.getAppPath() + "\n\n")
	scriptBuilder.WriteString("export APP_PATH=\"" + gen.getAppPath() + "\"\n")
	
	// set host:port
	common.SetEnvironmentVariableInScript(&scriptBuilder, "HOST", "", DefaultHost)
	common.SetEnvironmentVariableInScript(&scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)

	scriptBuilder.WriteString("./oryxBuildBinary\n\n")

	return scriptBuilder.String()
}

func (gen *GolangStartupScriptGenerator) getAppPath() string {
	if gen.Manifest.CompressDestinationDir == "true" && gen.Manifest.SourceDirectoryInBuildContainer != "" {
		return gen.Manifest.SourceDirectoryInBuildContainer
	}

	return gen.AppPath
}