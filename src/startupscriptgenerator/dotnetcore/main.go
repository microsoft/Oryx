// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"flag"
	"log"
	"path/filepath"
	"startupscriptgenerator/common"
)

func main() {
	common.PrintVersionInfo()

	sourcePathPtr := flag.String(
		"sourcePath",
		".",
		"The path to the application that is being deployed, e.g. '/home/site/repository/src/ShoppingWebApp/'.")
	intermediatePathPtr := flag.String(
		"intermediatePath",
		"",
		"The path to the directory where the output is copied and run from there.")
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
		"[Optional] Path to a default dll that will be executed if the entrypoint is not found."+
			" Ex: '/opt/startup/aspnetcoredefaultapp.dll'")
	flag.Parse()

	fullSourcePath := common.GetValidatedFullPath(*sourcePathPtr)

	common.SetGlobalOperationId(fullSourcePath)

	fullIntermediatePath := ""
	if *intermediatePathPtr == "" {
		fullIntermediatePath = "/tmp/output"
	} else {
		fullIntermediatePath = common.GetValidatedFullPath(*intermediatePathPtr)
	}

	fullPublishedOutputPath := ""
	if *publishedOutputPathPtr != "" {
		fullPublishedOutputPath = common.GetValidatedFullPath(*publishedOutputPathPtr)
	}

	fullDefaultAppFilePath := ""
	if *defaultAppFilePathPtr != "" {
		fullDefaultAppFilePath = common.GetValidatedFullPath(*defaultAppFilePathPtr)
	}

	srcFolder := fullPublishedOutputPath
	if srcFolder == "" {
		// The output folder is a sub-directory of this source directory
		srcFolder = filepath.Join(fullSourcePath, "oryx_publish_output")
	}
	buildManifest := common.GetBuildManifest(srcFolder)
	if buildManifest.ZipAllOutput == "true" {
		common.ExtractZippedOutput(srcFolder, fullIntermediatePath)
	} else {
		common.CopyOutputToIntermediateDir(srcFolder, fullIntermediatePath)
	}
	fullPublishedOutputPath = fullIntermediatePath

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
