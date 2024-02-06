// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"fmt"
	"path/filepath"
	"strings"
)

type RubyStartupScriptGenerator struct {
	SourcePath         string
	UserStartupCommand string
	DefaultAppFilePath string
	BindPort           string
	RailEnv            string
	Manifest           common.BuildManifest
	Configuration      Configuration
}

const DefaultBindPort = "8080"
const DefaultHost = "0.0.0.0"

func (gen *RubyStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("ruby.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	rubyInstallationRoot := fmt.Sprintf("/opt/ruby/%s", gen.Manifest.RubyVersion)
	logger.LogInformation("Generating script for source.")
	common.PrintVersionInfo()

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")

	enableDynamicInstall := common.GetBooleanEnvironmentVariable(consts.EnableDynamicInstallKey)
	if enableDynamicInstall && !common.PathExists(rubyInstallationRoot) {
		scriptBuilder.WriteString(fmt.Sprintf("oryx setupEnv -appPath %s\n", gen.SourcePath))
	}

	common.SetupPreRunScript(&scriptBuilder, gen.SourcePath, gen.Configuration.PreRunCommand)

	scriptBuilder.WriteString("\n# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd \"" + gen.SourcePath + "\"\n\n")

	// Expose the port so that a custom command can use it if needed.
	common.SetEnvironmentVariableInScript(&scriptBuilder, "HOST", "", DefaultHost)
	common.SetEnvironmentVariableInScript(&scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)

	scriptBuilder.WriteString(fmt.Sprintf("export PATH=\"%s/bin:${PATH}\"\n", rubyInstallationRoot))

	if gen.RailEnv == "" {
		scriptBuilder.WriteString("export RAILS_ENV=\"production\"\n")
	} else {
		scriptBuilder.WriteString(fmt.Sprintf("export RAILS_ENV=\"%s\"\n", gen.RailEnv))
	}

	extensibleCommands := common.ParseExtensibleConfigFile(filepath.Join(gen.SourcePath, consts.ExtensibleConfigurationFileName))
	if extensibleCommands != "" {
		logger.LogInformation("Found extensible configuration file to be used in the generated run script")
		scriptBuilder.WriteString(extensibleCommands)
	}

	command := gen.UserStartupCommand // A custom command takes precedence over any framework defaults
	if command != "" {
		isPermissionAdded := common.ParseCommandAndAddExecutionPermission(gen.UserStartupCommand, gen.SourcePath)
		logger.LogInformation("Permission added: %t", isPermissionAdded)
		logger.LogInformation("Running user supplied startup command.")
		command = common.ExtendPathForCommand(command, gen.SourcePath)
		scriptBuilder.WriteString("echo 'Running the command: " + command + "'\n")
		scriptBuilder.WriteString(command + "\n")
	} else {
		scriptBuilder.WriteString("echo 'Running the command: bundle install'\n")
		scriptBuilder.WriteString("bundle install\n")
		command := "bundle exec rails server"
		scriptBuilder.WriteString("echo 'Running the command: " + command + "'\n")
		scriptBuilder.WriteString(command + "\n")
	}

	var runScript = scriptBuilder.String()
	return runScript
}
