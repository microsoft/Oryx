// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"flag"
	"startupscriptgenerator/common"
)

func main() {
	sourcePathPtr := flag.String(
		"sourcePath",
		".",
		"The path to the application that is being deployed, e.g. '/home/site/repository/src/ShoppingWebApp/'.")
	publishedOutputPathPtr := flag.String(
		"publishedOutputPath",
		"",
		"The path to the published output of the application that is going to be run, e.g. '/home/site/wwwroot/'.")
	userStartupCommandPtr := flag.String(
		"userStartupCommand",
		"",
		"[Optional] Command that will be executed to start the application up.")
	outputPathPtr := flag.String("output", "run.sh", "Path to the script to be generated.")
	defaultAppFilePathPtr := flag.String(
		"defaultAppFilePath",
		"",
		"[Optional] Path to a default dll that will be executed if the entrypoint is not found. Ex: '/opt/startup/aspnetcoredefaultapp.dll'")
	flag.Parse()

	fullSourcePath := common.GetValidatedFullPath(*sourcePathPtr)

	fullPublishedOutputPath := ""
	if *publishedOutputPathPtr != "" {
		fullPublishedOutputPath = common.GetValidatedFullPath(*publishedOutputPathPtr)
	}

	fullDefaultAppFilePath := ""
	if *defaultAppFilePathPtr != "" {
		fullDefaultAppFilePath = common.GetValidatedFullPath(*defaultAppFilePathPtr)
	}

	entrypointGenerator := DotnetCoreStartupScriptGenerator{
		SourcePath:          fullSourcePath,
		PublishedOutputPath: fullPublishedOutputPath,
		UserStartupCommand:  *userStartupCommandPtr,
		DefaultAppFilePath:  fullDefaultAppFilePath,
	}

	command := entrypointGenerator.GenerateEntrypointScript()
	common.WriteScript(*outputPathPtr, command)
}
