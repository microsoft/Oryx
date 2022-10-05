// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"flag"
	"fmt"
	"log"
	"os"
	"path/filepath"
	"strings"
)

func main() {
	versionCommand := flag.NewFlagSet(consts.VersionCommandName, flag.ExitOnError)

	setupEnvCommand := flag.NewFlagSet(consts.SetupEnvCommandName, flag.ExitOnError)
	setupEnvAppPathPtr := setupEnvCommand.String(
		"appPath",
		".",
		"The path to the application folder, e.g. '/home/site/wwwroot/'.")

	scriptCommand := flag.NewFlagSet(consts.CreateScriptCommandName, flag.ExitOnError)
	appPathPtr := scriptCommand.String(
		"appPath",
		".",
		"The path to the published output of the application that is going to be run, e.g. '/home/site/wwwroot/'. "+
			"Default is current directory.")
	runFromPathPtr := scriptCommand.String(
		"runFromPath",
		"",
		"The path to the directory where the output is copied and run from there.")
	manifestDirPtr := scriptCommand.String(
		"manifestDir",
		"",
		"[Optional] Path to the directory where build manifest file can be found. If no value is provided, then "+
			"it is assumed to be under the directory specified by 'appPath'.")
	bindPortPtr := scriptCommand.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 8080")
	bindPort2Ptr := scriptCommand.String("bindPort2", "", "[Optional] .NET only - HTTP/2 port where the application will bind to. By default, no HTTP/2 binding will be made")
	userStartupCommandPtr := scriptCommand.String(
		"userStartupCommand",
		"",
		"[Optional] Command that will be executed to start the application up.")
	outputPathPtr := scriptCommand.String("output", "run.sh", "Path to the script to be generated.")
	defaultAppFilePathPtr := scriptCommand.String(
		"defaultAppFilePath",
		"",
		"[Optional] Path to a default dll that will be executed if the entrypoint is not found. "+
			"Ex: '/opt/startup/aspnetcoredefaultapp.dll'")

	logger := common.GetLogger("dotnetcore.main")
	defer logger.Shutdown()
	logger.StartupScriptRequested()

	commands := []*flag.FlagSet{versionCommand, scriptCommand, setupEnvCommand}
	common.ValidateCommands(commands)

	if scriptCommand.Parsed() {
		fullAppPath := ""
		if *appPathPtr != "" {
			providedPath := *appPathPtr
			absPath, err := filepath.Abs(providedPath)
			if err != nil || !common.PathExists(absPath) {
				fmt.Printf("Provided app path '%s' is not valid or does not exist.\n", providedPath)
				os.Exit(consts.FAILURE_EXIT_CODE)
			}
			fullAppPath = absPath
		}

		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
		common.SetGlobalOperationID(buildManifest)

		fullRunFromPath := ""
		if *runFromPathPtr != "" {
			// NOTE: This path might not exist, so do not try to validate it yet.
			fullRunFromPath, _ = filepath.Abs(*runFromPathPtr)
		}

		fullOutputPath := ""
		if *outputPathPtr != "" {
			// NOTE: This path might not exist, so do not try to validate it yet.
			fullOutputPath, _ = filepath.Abs(*outputPathPtr)
		}

		fullDefaultAppFilePath := ""
		if *defaultAppFilePathPtr != "" {
			providedPath := *defaultAppFilePathPtr
			absPath, err := filepath.Abs(providedPath)
			if err != nil || !common.FileExists(absPath) {
				fmt.Printf("Provided default app file path '%s' is not valid or does not exist.\n", providedPath)
				os.Exit(consts.FAILURE_EXIT_CODE)
			}
			fullDefaultAppFilePath = absPath
		}

		var configuration Configuration
		viperConfig := common.GetViperConfiguration(fullAppPath)
		configuration.AppInsightsAgentExtensionVersion = getAppInsightsAgentVersion(configuration)
		configuration.EnableDynamicInstall = viperConfig.GetBool(consts.EnableDynamicInstallKey)
		configuration.PreRunCommand = viperConfig.GetString(consts.PreRunCommandEnvVarName)

		scriptBuilder := strings.Builder{}
		scriptBuilder.WriteString("#!/bin/bash\n")
		scriptBuilder.WriteString("set -e\n\n")

		if fullRunFromPath != "" {
			fmt.Println(
				"Intermediate directory option was specified, so adding script to copy " +
					"content to intermediate directory...")
			common.AppendScriptToCopyToDir(&scriptBuilder, fullAppPath, fullRunFromPath)
		}

		entrypointGenerator := DotnetCoreStartupScriptGenerator{
			AppPath:            fullAppPath,
			RunFromPath:        fullRunFromPath,
			BindPort:           *bindPortPtr,
			BindPort2:          *bindPort2Ptr,
			UserStartupCommand: *userStartupCommandPtr,
			DefaultAppFilePath: fullDefaultAppFilePath,
			Manifest:           buildManifest,
			Configuration:      configuration,
		}

		command := entrypointGenerator.GenerateEntrypointScript(&scriptBuilder)
		if command == "" {
			log.Fatal("Could not generate a startup script.")
		}

		common.WriteScript(fullOutputPath, command)

		userRunCommand := common.ParseUserRunCommand(filepath.Join(fullAppPath, consts.AppSvcFileName))
		common.AppendScript(fullOutputPath, userRunCommand)
	}

	if setupEnvCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*setupEnvAppPathPtr)
		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
		dotNetCoreInstallationRoot := "/usr/share/dotnet"
		script := common.GetSetupScript(
			"dotnet",
			buildManifest.DotNetCoreSdkVersion,
			dotNetCoreInstallationRoot)
		scriptBuilder := strings.Builder{}
		scriptBuilder.WriteString(script)
		scriptBuilder.WriteString("ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet\n")
		finalScript := scriptBuilder.String()
		fmt.Println(fmt.Sprintf(
			"Setting up the environment with '.NET Core' version '%s'...\n",
			buildManifest.DotNetCoreSdkVersion))
		common.SetupEnv(finalScript)
	}
}

func getAppInsightsAgentVersion(configuration Configuration) string {
	// viper currently cannot read lower-case based environment variables which is a problem for us
	// due to the environment variable 'ApplicationInsightsAgent_EXTENSION_VERSION'
	// https://github.com/spf13/viper/issues/302
	// As a workaround, for this particular environment variable, we will depend on viper to read from
	// the config file but use regular 'os.Getenv' api to be able to read the lower case environment
	// variable
	valueFromViper := configuration.AppInsightsAgentExtensionVersion
	valueFromEnvVariable := os.Getenv(consts.UserAppInsightsAgentExtensionVersion)
	if valueFromEnvVariable == "" {
		// following represents value from config
		return valueFromViper
	} else {
		return valueFromEnvVariable
	}
}
