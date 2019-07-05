// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"flag"
)

func main() {
	common.PrintVersionInfo()

	appPathPtr := flag.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	manifestDirPtr := common.ManifestDirFlag
	startupCmdPtr := flag.String("startupCommand", "apache2-foreground", "Command that will be executed to start the application server up.")
	bindPortPtr := flag.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 8080")
	outputPathPtr := flag.String("output", "run.sh", "Path to the script to be generated.")
	flag.Parse()

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
