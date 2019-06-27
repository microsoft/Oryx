// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"fmt"
	"os"
	"path/filepath"
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
const RuntimeConfigJsonExtension = ".runtimeconfig.json"

func (gen *DotnetCoreStartupScriptGenerator) GenerateEntrypointScript(scriptBuilder *common.ScriptBuilder) string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for published output at '%s'", gen.AppPath)

	// Expose the port so that a custom command can use it if needed
	common.SetEnvironmentVariableInScript(scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)
	scriptBuilder.ExportVariable("ASPNETCORE_URLS", "http://*:$PORT")
	scriptBuilder.AppendEmptyLine()

	builtByOryx := false
	if common.ManifestFileExists(gen.AppPath) {
		builtByOryx = true
	}

	appPath := gen.AppPath
	if gen.RunFromPath != "" {
		appPath = gen.RunFromPath
	}

	runDefaultApp := false
	if gen.UserStartupCommand != "" {
		// NOTE: do NOT try printing the command itself
		scriptBuilder.Echo("Running user provided startup command...")
		scriptBuilder.ChangeDirectory(appPath)

		if gen.DefaultAppFilePath == "" {
			scriptBuilder.AppendLine(gen.UserStartupCommand)
		} else {
			defaultAppFileDir := filepath.Dir(gen.DefaultAppFilePath)
			scriptBuilder.AppendLine("EXIT_CODE=0")
			scriptBuilder.AppendLine(gen.UserStartupCommand + " || EXIT_CODE=$?")
			scriptBuilder.AppendLine("if [ $EXIT_CODE != 0 ]; then")
			scriptBuilder.AppendLine("    echo \"WARNING: Startup command execution failed with exit code $EXIT_CODE\"")
			scriptBuilder.AppendLine("    echo \"Running the default application instead...\"")
			scriptBuilder.AppendLine("    cd \"" + defaultAppFileDir + "\"")
			scriptBuilder.AppendLine("    dotnet \"" + gen.DefaultAppFilePath + "\"")
			scriptBuilder.AppendLine("fi")
		}
	} else {
		if builtByOryx {
			startupCommand := "dotnet \"" + gen.Manifest.StartupDllFileName + "\""
			scriptBuilder.Echo("'Running the command: " + startupCommand + "'")
			scriptBuilder.ChangeDirectory(appPath)
			scriptBuilder.AppendLine(startupCommand)
		} else {
			scriptBuilder.AppendLine("echo Trying to find the startup dll name...")
			runtimeConfigFiles := gen.getRuntimeConfigJsonFiles()

			if len(runtimeConfigFiles) == 0 {
				fmt.Printf(
					"WARNING: Unable to find the startup dll name. Could not find any files with extension '%s'\n",
					RuntimeConfigJsonExtension)
				runDefaultApp = true
			} else if len(runtimeConfigFiles) > 1 {
				fmt.Printf(
					"WARNING: Expected to find only one file with extension '%s' but found %d\n",
					RuntimeConfigJsonExtension,
					len(runtimeConfigFiles))
				fmt.Printf("WARNING: Found files: '%s'\n", strings.Join(runtimeConfigFiles, ", "))
				fmt.Println("WARNING: To fix this issue you can set the startup command to point to a particular startup file")
				fmt.Println("         For example: 'dotnet myapp.dll'")
				runDefaultApp = true
			} else {
				runtimeConfigFile := runtimeConfigFiles[0]
				startupDllName := strings.TrimSuffix(runtimeConfigFile, RuntimeConfigJsonExtension) + ".dll"
				startupDllFullPath := filepath.Join(appPath, startupDllName)
				if common.FileExists(startupDllFullPath) {
					startupCommand := "dotnet \"" + startupDllName + "\""
					scriptBuilder.Echo("Found the startup dll name: " + startupDllName)
					scriptBuilder.Echo("'Running the command: " + startupCommand + "'")
					scriptBuilder.ChangeDirectory(appPath)
					scriptBuilder.AppendLine(startupCommand)
				} else {
					fmt.Printf(
						"WARNING: Unable to figure out startup dll name. Found file '%s', but could not find startup file '%s' on disk.\n",
						runtimeConfigFile,
						startupDllFullPath)
					runDefaultApp = true
				}
			}
		}
	}

	if runDefaultApp && gen.DefaultAppFilePath != "" {
		defaultAppFileDir := filepath.Dir(gen.DefaultAppFilePath)
		startupCommand := "dotnet \"" + gen.DefaultAppFilePath + "\""
		scriptBuilder.Echo("'Running the default app using command: " + startupCommand + "'")
		scriptBuilder.ChangeDirectory(defaultAppFileDir)
		scriptBuilder.AppendLine(startupCommand)
	}

	var runScript = scriptBuilder.ToString()
	logger.LogInformation("Run script content:\n" + runScript)
	return runScript
}

func (gen *DotnetCoreStartupScriptGenerator) getRuntimeConfigJsonFiles() []string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.getRuntimeConfigJsonFiles")
	defer logger.Shutdown()

	fileList := make([]string, 0)

	var appDir *os.File
	var files []os.FileInfo
	var err error
	appDir, err = os.Open(gen.AppPath)
	defer appDir.Close()
	if err == nil {
		files, err = appDir.Readdir(-1)
	}

	if err != nil {
		fmt.Printf(
			"An error occurred while trying to look for '%s' files under '%s'.\n",
			RuntimeConfigJsonExtension,
			appDir.Name())
		logger.LogError("An error occurred while trying to look for '%s' files under '%s'. Exception: '%s'",
			RuntimeConfigJsonExtension,
			appDir.Name(),
			err)
		return fileList
	}

	for _, file := range files {
		if file.Mode().IsRegular() {
			fileName := file.Name()
			if strings.HasSuffix(fileName, RuntimeConfigJsonExtension) {
				fileList = append(fileList, fileName)
			}
		}
	}
	return fileList
}
