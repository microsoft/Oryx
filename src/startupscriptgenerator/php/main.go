// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"flag"
	"startupscriptgenerator/common"
)

const DefaultBindPort = "8080"

func main() {
	common.PrintVersionInfo()

	appPathPtr := flag.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	startupCmdPtr := flag.String("startupCommand", "apache2-foreground", "Command that will be executed to start the application server up.")
	bindPortPtr := flag.String("bindPort", DefaultBindPort, "[Optional] Port where the application will bind to.")
	outputPathPtr := flag.String("output", "run.sh", "Path to the script to be generated.")
	flag.Parse()

	fullAppPath := common.GetValidatedFullPath(*appPathPtr)

	common.SetGlobalOperationId(fullAppPath)

	entrypointGenerator := PhpStartupScriptGenerator{
		SourcePath: fullAppPath,
		StartupCmd: *startupCmdPtr,
		BindPort:   *bindPortPtr,
	}

	command := entrypointGenerator.GenerateEntrypointScript()
	common.WriteScript(*outputPathPtr, command)
}
