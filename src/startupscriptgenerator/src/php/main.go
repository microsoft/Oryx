// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"flag"
	"os"
)

func main() {
	common.PrintVersionInfo()
	appPathPtr := flag.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	manifestDirPtr := flag.String(
		"manifestDir",
		"",
		"[Optional] Path to the directory where build manifest file can be found. If no value is provided, then "+
			"it is assumed to be under the directory specified by 'appPath'.")

	var _phpOrigin = os.Getenv(consts.PhpOriginEnvVarName)
	var startupCommand = ""

	if _phpOrigin == "php-fpm" {
		startupCommand = "php-fpm"
	} else {
		startupCommand = "apache2-foreground"
	}

	startupCmdPtr := flag.String("startupCommand", startupCommand, "Command that will be executed to start the application server up.")
	bindPortPtr := flag.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 8080")
	outputPathPtr := flag.String("output", "run.sh", "Path to the script to be generated.")
	flag.Parse()

	logger := common.GetLogger("php.main")
	defer logger.Shutdown()
	logger.StartupScriptRequested()

	fullAppPath := common.GetValidatedFullPath(*appPathPtr)

	buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
	common.SetGlobalOperationID(buildManifest)

	entrypointGenerator := PhpStartupScriptGenerator{
		SourcePath: fullAppPath,
		StartupCmd: *startupCmdPtr,
		BindPort:   *bindPortPtr,
		Manifest:   buildManifest,
	}

	command := entrypointGenerator.GenerateEntrypointScript()
	common.WriteScript(*outputPathPtr, command)
}
