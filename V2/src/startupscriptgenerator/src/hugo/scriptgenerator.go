// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"strings"
)

type HugoStartupScriptGenerator struct {
	AppPath                  string
	UserStartupCommand       string
	DefaultAppPath           string
	BindPort                 string
	Manifest                 common.BuildManifest
	Configuration            Configuration
}

func (gen *HugoStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("hugo.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogVerbose("Remove this LogVerbose message")
	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n\n")
	scriptBuilder.WriteString("echo TODO: update with hugo script commands")

	command := gen.UserStartupCommand // A custom command takes precedence over any framework defaults
	if command != "" {
		isPermissionAdded := common.ParseCommandAndAddExecutionPermission(gen.UserStartupCommand, gen.AppPath)
		logger.LogInformation("Permission added: %t", isPermissionAdded)
		command = common.ExtendPathForCommand(command, gen.AppPath)
	}
	return scriptBuilder.String()
}

func (gen *HugoStartupScriptGenerator) getAppPath() string {
	if gen.Manifest.CompressDestinationDir == "true" && gen.Manifest.SourceDirectoryInBuildContainer != "" {
		return gen.Manifest.SourceDirectoryInBuildContainer
	}

	return gen.AppPath
}