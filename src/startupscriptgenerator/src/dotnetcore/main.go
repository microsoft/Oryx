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
	common.PrintVersionInfo()

	// Subcommands
	setupEnvCommand := flag.NewFlagSet(consts.SetupEnvCommandName, flag.ExitOnError)
	setupEnvAppPathPtr := setupEnvCommand.String(
		"appPath",
		".",
		"The path to the published output of the application that is going to be run, e.g. '/home/site/wwwroot/'. "+
			"Default is current directory.")

	scriptCommand := flag.NewFlagSet(consts.ScriptCommandName, flag.ExitOnError)
	appPathPtr := scriptCommand.String(
		"appPath",
		".",
		"The path to the published output of the application that is going to be run, e.g. '/home/site/wwwroot/'. "+
			"Default is current directory.")
	runFromPathPtr := scriptCommand.String(
		"runFromPath",
		"",
		"The path to the directory where the output is copied and run from there.")
	manifestDirPtr := common.ManifestDirFlag
	bindPortPtr := scriptCommand.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 8080")
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
	setupEnvironment := scriptCommand.Bool(
		consts.SetupEnvCommandName,
		false,
		"If true, adds content to setup the environment before running the app")

	common.ValidateCommands(scriptCommand, setupEnvCommand)

	logger := common.GetLogger("dotnetcore.main")
	defer logger.Shutdown()
	logger.StartupScriptRequested()

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

		scriptBuilder := strings.Builder{}
		scriptBuilder.WriteString("#!/bin/bash\n")
		scriptBuilder.WriteString("set -e\n\n")

		if fullRunFromPath != "" {
			fmt.Println(
				"Intermediate directory option was specified, so adding script to copy " +
					"content to intermediate directory...")
			common.AppendScriptToCopyToDir(&scriptBuilder, fullAppPath, fullRunFromPath)
		}

		if buildManifest.ZipAllOutput == "true" {
			fmt.Println(
				"Read build manifest file and found output has been zipped, so adding " +
					"script to extract it...")
			common.AppendScriptToExtractZippedOutput(&scriptBuilder, fullAppPath, fullRunFromPath)
		}

		entrypointGenerator := DotnetCoreStartupScriptGenerator{
			AppPath:            fullAppPath,
			RunFromPath:        fullRunFromPath,
			BindPort:           *bindPortPtr,
			UserStartupCommand: *userStartupCommandPtr,
			DefaultAppFilePath: fullDefaultAppFilePath,
			Manifest:           buildManifest,
			SetupEnvironment:   *setupEnvironment,
		}

		command := entrypointGenerator.GenerateEntrypointScript(&scriptBuilder)
		if command == "" {
			log.Fatal("Could not generate a startup script.")
		}

		common.WriteScript(fullOutputPath, command)
	}

	if setupEnvCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*setupEnvAppPathPtr)
		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
		script := common.GetSetupScript(
			"dotnet",
			buildManifest.DotNetCoreSdkVersion,
			consts.DotNetCoreInstallationDir)
		fmt.Println(fmt.Sprintf(
			"Setting up the environment with 'dotnetcore' version '%s'...\n",
			buildManifest.DotNetCoreSdkVersion))
		common.SetupEnv(script)
	}
}
