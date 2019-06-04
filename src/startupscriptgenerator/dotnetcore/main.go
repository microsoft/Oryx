// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"flag"
	"fmt"
	"log"
	"path/filepath"
	"startupscriptgenerator/common"
	"strings"
)

func main() {
	common.PrintVersionInfo()

	appPathPtr := flag.String(
		"appPath",
		".",
		"The path to the published output of the application or the root of the source that is going to be run, e.g. '/home/site/wwwroot/'. or '/home/site/repository' "+
			"Default is current directory.")
	runFromPathPtr := flag.String(
		"runFromPath",
		"",
		"The path to the directory where the output is copied and run from there.")
	bindPortPtr := flag.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 8080")
	userStartupCommandPtr := flag.String(
		"userStartupCommand",
		"",
		"[Optional] Command that will be executed to start the application up.")
	outputPathPtr := flag.String("output", "run.sh", "Path to the script to be generated.")
	defaultAppFilePathPtr := flag.String(
		"defaultAppFilePath",
		"",
		"[Optional] Path to a default dll that will be executed if the entrypoint is not found. "+
			"Ex: '/opt/startup/aspnetcoredefaultapp.dll'")
	flag.Parse()

	fullAppPath := ""
	if *appPathPtr != "" {
		fullAppPath = common.GetValidatedFullPath(*appPathPtr)
	}

	// If a user supplies the repo root (instead of output folder), we try to see if the repo root has
	// the default oryx publish output folder
	defaultPublishOutputDirPath := filepath.Join(fullAppPath, "oryx_publish_output")
	if common.PathExists(defaultPublishOutputDirPath) {
		fmt.Printf(
			"Found publish output directory at '%s'. Using it's contents for running the app.\n", 
			defaultPublishOutputDirPath)
		fullAppPath = defaultPublishOutputDirPath
	}

	common.SetGlobalOperationId(fullAppPath)

	fullRunFromPath := ""
	if *runFromPathPtr != "" {
		// NOTE: This path might not exist, so do not try to validate it yet.
		fullRunFromPath, _ = filepath.Abs(*runFromPathPtr)
	}

	fullDefaultAppFilePath := ""
	if *defaultAppFilePathPtr != "" {
		fullDefaultAppFilePath = common.GetValidatedFullPath(*defaultAppFilePathPtr)
	}

	if fullDefaultAppFilePath != "" && !common.FileExists(fullDefaultAppFilePath) {
		fmt.Printf("Supplied default app file path '%s' does not exist", fullDefaultAppFilePath)
		return
	}

	fmt.Println("Building the startup script...")
	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/bash\n")
	scriptBuilder.WriteString("set -e\n\n")
	scriptBuilder.WriteString("echo Running the script...\n\n")

	if fullRunFromPath != "" {
		fmt.Println(
			"Intermediate directory option was specified, so adding script to copy " +
				"content to intermediate directory...")
		common.AppendScriptToCopyToDir(&scriptBuilder, fullAppPath, fullRunFromPath)
	}

	if fullRunFromPath == "" {
		fullRunFromPath = fullAppPath
	}

	// Note that we read the manifest file from the AppPath since the content of AppPath
	// has not been copied to RunFromPath
	buildManifest := common.GetBuildManifest(fullAppPath)
	if buildManifest.ZipAllOutput == "true" {
		fmt.Println(
			"Read build manifest file and found output has been zipped, so adding " +
				"script to extract it...")
		common.AppendScriptToExtractZippedOutput(&scriptBuilder, fullRunFromPath)
	}

	entrypointGenerator := DotnetCoreStartupScriptGenerator{
		AppPath:            fullAppPath,
		RunFromPath:        fullRunFromPath,
		BindPort:           *bindPortPtr,
		UserStartupCommand: *userStartupCommandPtr,
		DefaultAppFilePath: fullDefaultAppFilePath,
		Manifest:           buildManifest,
	}

	command := entrypointGenerator.GenerateEntrypointScript(&scriptBuilder)
	if command == "" {
		log.Fatal("Could not generate a startup script.")
	}

	common.WriteScript(*outputPathPtr, command)
}
