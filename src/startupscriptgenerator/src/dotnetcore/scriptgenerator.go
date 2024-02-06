// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"fmt"
	"os"
	"path/filepath"
	"strings"

	"github.com/Masterminds/semver"
)

type DotnetCoreStartupScriptGenerator struct {
	AppPath            string
	RunFromPath        string
	UserStartupCommand string
	DefaultAppFilePath string
	BindPort           string
	BindPort2          string
	Manifest           common.BuildManifest
	Configuration      Configuration
}

const DefaultBindPort = "8080"
const RuntimeConfigJsonExtension = ".runtimeconfig.json"

// Checks if the application insights needs to be enabled for the current runtime
func (gen *DotnetCoreStartupScriptGenerator) shouldApplicationInsightsBeConfigured() bool {
	// Check if the application insights environment variables are present
	appInsightsAgentExtensionVersionEnv := gen.Configuration.AppInsightsAgentExtensionVersion
	dotNetRuntimeVersion := ""
	fmt.Printf("\nAgent extension %s", gen.Configuration.AppInsightsAgentExtensionVersion)
	fmt.Printf("\nBefore if loop >> DotNet Runtime %s", gen.Manifest.DotNetCoreRuntimeVersion)

	if gen.Manifest.DotNetCoreRuntimeVersion != "" {
		dotNetRuntimeVersion = gen.Manifest.DotNetCoreRuntimeVersion
	} else {
		dotNetRuntimeVersion = os.Getenv("DOTNET_VERSION")
	}

	fmt.Printf("\nDotNet Runtime %s", dotNetRuntimeVersion)

	if dotNetRuntimeVersion != "" {
		dotNetAppInsightsSupportedVersionConstraint, err := semver.NewConstraint(">= 6.0.0-0")
		if err != nil {
			fmt.Printf("\nError in creating semver constraint %s", err)
		}

		dotNetCurrentVersion, err := semver.NewVersion(dotNetRuntimeVersion)
		if err != nil {
			fmt.Printf("\nError in parsing current version to semver version %s", err)
		}
		// Check if the version meets the constraints. The a variable will be true.
		constraintCheckResult := dotNetAppInsightsSupportedVersionConstraint.Check(dotNetCurrentVersion)

		if constraintCheckResult &&
			appInsightsAgentExtensionVersionEnv != "" &&
			appInsightsAgentExtensionVersionEnv == "~3" {
			fmt.Printf("\nBefore returning true")
			return true
		}
	}

	return false
}

// Checks if the .NET runtime version meets the version contraint
func (gen *DotnetCoreStartupScriptGenerator) isDotnetRuntimeVersionMeetConstraint(versionConstraint string) bool {
	dotNetRuntimeVersion := ""

	if gen.Manifest.DotNetCoreRuntimeVersion != "" {
		dotNetRuntimeVersion = gen.Manifest.DotNetCoreRuntimeVersion
	} else {
		dotNetRuntimeVersion = os.Getenv("DOTNET_VERSION")
	}

	fmt.Printf("\nDotNet Runtime %s", dotNetRuntimeVersion)

	if dotNetRuntimeVersion != "" {
		dotNetRuntimeVersionConstraint, err := semver.NewConstraint(">= " + versionConstraint)
		if err != nil {
			fmt.Printf("\nError in creating semver constraint %s", err)
			return false
		}

		dotNetCurrentVersion, err := semver.NewVersion(dotNetRuntimeVersion)
		if err != nil {
			fmt.Printf("\nError in parsing current version to semver version %s", err)
			return false
		}
		// Check if the version meets the constraints. The a variable will be true.
		constraintCheckResult := dotNetRuntimeVersionConstraint.Check(dotNetCurrentVersion)

		return constraintCheckResult
	}

	return false
}

func (gen *DotnetCoreStartupScriptGenerator) GenerateEntrypointScript(scriptBuilder *strings.Builder) string {
	logger := common.GetLogger("dotnetcore.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for published output.")

	common.SetupPreRunScript(scriptBuilder, gen.AppPath, gen.Configuration.PreRunCommand)

	// Expose the port so that a custom command can use it if needed
	common.SetEnvironmentVariableInScript(scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)
	if gen.BindPort2 != "" {
		scriptBuilder.WriteString("export PORT2=" + gen.BindPort2 + "\n\n")
	}
	scriptBuilder.WriteString("export ASPNETCORE_URLS=http://*:$PORT\n\n")

	logger.LogInformation("Setting up Kestrel Endpoints with BindPort and BindPort2 Env variables")

	scriptBuilder.WriteString("if [ ! -z \"$PORT2\" ]; then" + "\n")
	scriptBuilder.WriteString("		export Kestrel__Endpoints__Http2__Url=http://*:$PORT2\n")
	scriptBuilder.WriteString("		export Kestrel__Endpoints__Http2__Protocols=Http2\n")
	scriptBuilder.WriteString("		export Kestrel__Endpoints__Http1__Url=http://*:$PORT\n")
	scriptBuilder.WriteString("		export Kestrel__Endpoints__Http1__Protocols=Http1\n\n")
	scriptBuilder.WriteString("fi")
	scriptBuilder.WriteString("\n\n")

	appPath := gen.AppPath
	if gen.RunFromPath != "" {
		appPath = gen.RunFromPath
	}

	dotnetBinary := "/usr/share/dotnet/dotnet"

	if gen.Configuration.EnableDynamicInstall && !common.PathExists(dotnetBinary) {
		scriptBuilder.WriteString(fmt.Sprintf("oryx setupEnv -appPath %s\n", appPath))
	}

	logger.LogInformation("Looking for App-Insights configuration and Enable codeless attach if needed")
	if gen.shouldApplicationInsightsBeConfigured() {

		// We are going to set env variables in the startup logic
		// for appinsights attach experience only if dotnetcore 6 or newer
		fmt.Printf("Environment Variables for Application Insight's Codeless Configuration exists.. \n")
		fmt.Printf("Setting up Environment Variables for Application Insights for codeless config.. \n")
		scriptBuilder.WriteString("echo Setting up Application Insights for codeless config.. \n")
		scriptBuilder.WriteString("export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=" + consts.UserNetcoreHostingstartupAssemblies + "\n")
		scriptBuilder.WriteString("export DOTNET_STARTUP_HOOKS=" + consts.UserDotnetStartupHooks + "\n")
		fmt.Printf("Setting up Environment Variables for Application Insights is done.. \n")
	}

	extensibleCommands := common.ParseExtensibleConfigFile(filepath.Join(gen.AppPath, consts.ExtensibleConfigurationFileName))
	if extensibleCommands != "" {
		logger.LogInformation("Found extensible configuration file to be used in the generated run script")
		scriptBuilder.WriteString(extensibleCommands)
	}

	runDefaultApp := false
	if gen.UserStartupCommand != "" {
		// NOTE: do NOT try printing the command itself
		scriptBuilder.WriteString("echo Running user provided startup command...\n")
		scriptBuilder.WriteString("cd \"" + appPath + "\"\n")
		scriptBuilder.WriteString(gen.UserStartupCommand)
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
		logger.LogError("An error occurred while trying to look for '%s' files. Exception: '%s'",
			RuntimeConfigJsonExtension,
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
