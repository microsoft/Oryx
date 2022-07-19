// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"common/consts"
	"flag"
)

func main() {
	versionCommand := flag.NewFlagSet(consts.VersionCommandName, flag.ExitOnError)

	scriptCommand := flag.NewFlagSet(consts.CreateScriptCommandName, flag.ExitOnError)
	appPathPtr := scriptCommand.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	manifestDirPtr := scriptCommand.String(
		"manifestDir",
		"",
		"[Optional] Path to the directory where build manifest file can be found. If no value is provided, then "+
			"it is assumed to be under the directory specified by 'appPath'.")

	startupCmdPtr := scriptCommand.String("startupCommand", "", "Command that will be executed to start the application server up.")
	bindPortPtr := scriptCommand.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 8080")
	outputPathPtr := scriptCommand.String("output", "run.sh", "Path to the script to be generated.")

	logger := common.GetLogger("php.main")
	defer logger.Shutdown()
	logger.StartupScriptRequested()

	commands := []*flag.FlagSet{versionCommand, scriptCommand}
	common.ValidateCommands(commands)

	if scriptCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*appPathPtr)

		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
		common.SetGlobalOperationID(buildManifest)

		var configuration Configuration
		viperConfig := common.GetViperConfiguration(fullAppPath)
		configuration.PhpOrigin = viperConfig.GetString(consts.PhpOrigin)
		configuration.PreRunCommand = viperConfig.GetString(consts.PreRunCommandEnvVarName)
		configuration.FpmMaxChildren = viperConfig.GetString(consts.FpmMaxChildrenEnvVarName)
		configuration.FpmStartServers = viperConfig.GetString(consts.FpmStartServersEnvVarName)
		configuration.FpmMinSpareServers = viperConfig.GetString(consts.FpmMinSpareServersEnvVarName)
		configuration.FpmMaxSpareServers = viperConfig.GetString(consts.FpmMaxSpareServersEnvVarName)

		entrypointGenerator := PhpStartupScriptGenerator{
			SourcePath:    fullAppPath,
			StartupCmd:    *startupCmdPtr,
			BindPort:      *bindPortPtr,
			Manifest:      buildManifest,
			Configuration: configuration,
		}

		command := entrypointGenerator.GenerateEntrypointScript()
		common.WriteScript(*outputPathPtr, command)
	}
}
