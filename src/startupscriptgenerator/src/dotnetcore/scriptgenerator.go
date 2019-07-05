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

func (gen *DotnetCoreStartupScriptGenerator) GenerateEntrypointScript(scriptBuilder *strings.Builder) string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for published output at '%s'", gen.AppPath)

	// Expose the port so that a custom command can use it if needed
	common.SetEnvironmentVariableInScript(scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)
	scriptBuilder.WriteString("export ASPNETCORE_URLS=http://*:$PORT\n\n")

	appPath := gen.AppPath
	if gen.RunFromPath != "" {
		appPath = gen.RunFromPath
	}

	runDefaultApp := false
	if gen.UserStartupCommand != "" {
		// NOTE: do NOT try printing the command itself
		scriptBuilder.WriteString("echo Running user provided startup command...\n")
		scriptBuilder.WriteString("cd \"" + appPath + "\"\n")

		if gen.DefaultAppFilePath == "" {
			scriptBuilder.WriteString(gen.UserStartupCommand)
		} else {
			defaultAppFileDir := filepath.Dir(gen.DefaultAppFilePath)
			scriptBuilder.WriteString("EXIT_CODE=0\n")
			scriptBuilder.WriteString(gen.UserStartupCommand + " || EXIT_CODE=$?\n")
			scriptBuilder.WriteString("if [ $EXIT_CODE != 0 ]; then\n")
			scriptBuilder.WriteString("    echo \"WARNING: Startup command execution failed with exit code $EXIT_CODE\"\n")
			scriptBuilder.WriteString("    echo \"Running the default application instead...\"\n")
			scriptBuilder.WriteString("    cd \"" + defaultAppFileDir + "\"\n")
			scriptBuilder.WriteString("    dotnet \"" + gen.DefaultAppFilePath + "\"\n")
			scriptBuilder.WriteString("fi\n")
		}
	} else {
		if gen.Manifest.StartupDllFileName != "" {
			scriptBuilder.WriteString("echo Found startup DLL name from manifest file\n")
			startupCommand := "dotnet \"" + gen.Manifest.StartupDllFileName + "\""
			scriptBuilder.WriteString("echo 'Running the command: " + startupCommand + "'\n")
			scriptBuilder.WriteString("cd \"" + appPath + "\"\n")
			scriptBuilder.WriteString(startupCommand + "\n")
		} else {
			scriptBuilder.WriteString("echo Trying to find the startup DLL name...\n")
			runtimeConfigFiles := gen.getRuntimeConfigJsonFiles()

			if len(runtimeConfigFiles) == 0 {
				fmt.Printf(
					"WARNING: Unable to find the startup DLL name. Could not find any files with extension '%s'\n",
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
					scriptBuilder.WriteString("echo Found the startup D name: " + startupDllName + "\n")
					scriptBuilder.WriteString("echo 'Running the command: " + startupCommand + "'\n")
					scriptBuilder.WriteString("cd \"" + appPath + "\"\n")
					scriptBuilder.WriteString(startupCommand + "\n")
				} else {
					fmt.Printf(
						"WARNING: Unable to figure out startup D name. Found file '%s', but could not find startup file '%s' on disk.\n",
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
		scriptBuilder.WriteString("echo 'Running the default app using command: " + startupCommand + "'\n")
		scriptBuilder.WriteString("cd \"" + defaultAppFileDir + "\"\n")
		scriptBuilder.WriteString(startupCommand + "\n")
	}

	var runScript = scriptBuilder.String()
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
