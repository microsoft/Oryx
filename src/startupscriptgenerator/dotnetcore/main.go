// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"flag"
	"log"
	"os/exec"
	"startupscriptgenerator/common"
)

func main() {
	common.PrintVersionInfo()

	sourcePathPtr := flag.String(
		"sourcePath",
		".",
		"The path to the application that is being deployed, e.g. '/home/site/repository/src/ShoppingWebApp/'.")
	publishedOutputPathPtr := flag.String(
		"publishedOutputPath",
		"",
		"The path to the published output of the application that is going to be run, e.g. '/home/site/wwwroot/'.")
	bindPortPtr := flag.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 8080")
	userStartupCommandPtr := flag.String(
		"userStartupCommand",
		"",
		"[Optional] Command that will be executed to start the application up.")
	outputPathPtr := flag.String("output", "run.sh", "Path to the script to be generated.")
	defaultAppFilePathPtr := flag.String(
		"defaultAppFilePath",
		"",
		"[Optional] Path to a default dll that will be executed if the entrypoint is not found." +
		" Ex: '/opt/startup/aspnetcoredefaultapp.dll'")
	copyOutputToDifferentDirAndRunPtr := flag.String(
		"copyOutputToDifferentDirAndRun",
		"true",
		"Flag which determines if the application should run after copying the supplied 'publishedOutputPath' " +
		"content to a different directory and run from there. Default is true.")
	flag.Parse()

	fullSourcePath := common.GetValidatedFullPath(*sourcePathPtr)

	common.SetGlobalOperationId(fullSourcePath)

	fullPublishedOutputPath := ""
	if *publishedOutputPathPtr != "" {
		fullPublishedOutputPath = common.GetValidatedFullPath(*publishedOutputPathPtr)
	}

	fullDefaultAppFilePath := ""
	if *defaultAppFilePathPtr != "" {
		fullDefaultAppFilePath = common.GetValidatedFullPath(*defaultAppFilePathPtr)
	}

	if (*copyOutputToDifferentDirAndRunPtr == "true") {
		srcFolder := fullPublishedOutputPath
		if (srcFolder == "") {
			// The output folder is a sub-directory of this source directory
			srcFolder = fullSourcePath
		}

		destFolder := "/tmp/output"
		println("Copying content from '" + srcFolder + "' to '"+ destFolder + "'...")
		cpCmd := exec.Command("cp", "-rf", srcFolder, destFolder)
		err := cpCmd.Run()
		if err != nil {
			panic(err)
		}

		// Update the variables so the downstream code uses these updated paths
		if (fullPublishedOutputPath != "") {
			// if a publish output is given, we would have copied only that one to local folder
			fullPublishedOutputPath = destFolder
		} else {
			fullSourcePath = destFolder
		}
	}

	entrypointGenerator := DotnetCoreStartupScriptGenerator{
		SourcePath:          fullSourcePath,
		PublishedOutputPath: fullPublishedOutputPath,
		BindPort:            *bindPortPtr,
		UserStartupCommand:  *userStartupCommandPtr,
		DefaultAppFilePath:  fullDefaultAppFilePath,
	}

	command := entrypointGenerator.GenerateEntrypointScript()
	if command == "" {
		log.Fatal("Could not generate a startup script.")
	}

	common.WriteScript(*outputPathPtr, command)
}
