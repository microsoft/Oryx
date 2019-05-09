// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"path/filepath"
	"startupscriptgenerator/common"
	"strings"
)

type DotnetCoreStartupScriptGenerator struct {
	AppPath            string
	RunFromPath        string
	UserStartupCommand string
	DefaultAppFilePath string
	BindPort           string
	Manifest           common.BuildManifest
}

const DefaultBindPort = "8080"
const RuntimeConfigJsonExtension = "runtimeconfig.json"

func (gen *DotnetCoreStartupScriptGenerator) GenerateEntrypointScript(scriptBuilder *strings.Builder) string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for published output at '%s'", gen.AppPath)

	// Expose the port so that a custom command can use it if needed
	common.SetEnvironmentVariableInScript(scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)
	scriptBuilder.WriteString("export ASPNETCORE_URLS=http://*:$PORT\n\n")

	defaultAppFileDir := filepath.Dir(gen.DefaultAppFilePath)

	scriptBuilder.WriteString("readonly appPath=\"" + gen.RunFromPath + "\"\n")
	scriptBuilder.WriteString("userStartUpCommand=\"" + gen.UserStartupCommand + "\"\n")
	scriptBuilder.WriteString("startUpCommand=\"\"\n")
	scriptBuilder.WriteString("readonly defaultAppFileDir=\"" + defaultAppFileDir + "\"\n")
	scriptBuilder.WriteString("readonly defaultAppFilePath=\"" + gen.DefaultAppFilePath + "\"\n")
	scriptBuilder.WriteString("echo Finding the startup file name...\n")
	scriptBuilder.WriteString("cd \"$appPath\"\n")
	scriptBuilder.WriteString("for file in *; do \n")
	scriptBuilder.WriteString("  if [ -f \"$file\" ]; then \n")
	scriptBuilder.WriteString("    case $file in\n")
	scriptBuilder.WriteString("      *.runtimeconfig.json)\n")
	scriptBuilder.WriteString("        startupDllFileNamePrefix=${file%%.runtimeconfig.json}\n")
	scriptBuilder.WriteString("        startupDllFileName=\"$startupDllFileNamePrefix.dll\"\n")
	scriptBuilder.WriteString("  	     echo \"Found the startup file '$startupDllFileName'\"\n")
	scriptBuilder.WriteString("        break\n")
	scriptBuilder.WriteString("      ;;\n")
	scriptBuilder.WriteString("    esac\n")
	scriptBuilder.WriteString("  fi\n")
	scriptBuilder.WriteString("done\n")
	scriptBuilder.WriteString("if [ ! -z \"$userStartUpCommand\" ]; then\n")
	scriptBuilder.WriteString("  cd \"$appPath\"\n")
	scriptBuilder.WriteString("  startUpCommand=\"$userStartUpCommand\"\n")
	scriptBuilder.WriteString("  if [ -f \"$userStartUpCommand\" ]; then\n")
	scriptBuilder.WriteString("    chmod 755 \"$userStartUpCommand\"\n")
	scriptBuilder.WriteString("  fi\n")
	scriptBuilder.WriteString("elif [ -f \"$startupDllFileName\" ]; then\n")
	scriptBuilder.WriteString("  cd \"$appPath\"\n")
	scriptBuilder.WriteString("  startUpCommand=\"dotnet '$startupDllFileName'\"\n")
	scriptBuilder.WriteString("elif [ -f \"$defaultAppFilePath\" ]; then\n")
	scriptBuilder.WriteString("  cd \"$defaultAppFileDir\"\n")
	scriptBuilder.WriteString("  startUpCommand=\"dotnet '$defaultAppFilePath'\"\n")
	scriptBuilder.WriteString("else\n")
	scriptBuilder.WriteString("  echo Unable to start the application.\n")
	scriptBuilder.WriteString("  exit 1\n")
	scriptBuilder.WriteString("fi\n\n")
	scriptBuilder.WriteString("eval \"$startUpCommand\"\n\n")

	var runScript = scriptBuilder.String()
	logger.LogInformation("Run script content:\n" + runScript)
	return runScript
}
