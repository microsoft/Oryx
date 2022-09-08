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
		"The path to the application folder, e.g. '/home/site/wwwroot/'.")
	railEnvironment := scriptCommand.String(
		"railEnv",
		"",
		"[Optional] The rail environment, default to 'production'.")
	manifestDirPtr := scriptCommand.String(
		"manifestDir",
		"",
		"[Optional] Path to the directory where build manifest file can be found. If no value is provided, then "+
			"it is assumed to be under the directory specified by 'appPath'.")
	userStartupCommandPtr := scriptCommand.String(
		"userStartupCommand",
		"",
		"[Optional] Command that will be executed to start the application up.")
	defaultAppFilePathPtr := scriptCommand.String(
		"defaultApp",
		"",
		"[Optional] Path to a default file that will be executed if the entrypoint is not found. "+
			"Ex: '/opt/startup/default-static-site.js'")
	bindPortPtr := scriptCommand.String(
		"bindPort",
		"",
		"[Optional] Port where the application will bind to. Default is 8080")
	outputPathPtr := scriptCommand.String("output", "run.sh", "Path to the script to be generated.")

	flag.Parse()

	logger := common.GetLogger("ruby.main")
	defer logger.Shutdown()
	logger.StartupScriptRequested()

	commands := []*flag.FlagSet{versionCommand, scriptCommand, setupEnvCommand}
	common.ValidateCommands(commands)

	if scriptCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*appPathPtr)
		fullDefaultAppFilePath := common.GetValidatedFullPath(*defaultAppFilePathPtr)

		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
		common.SetGlobalOperationID(buildManifest)

		var configuration Configuration
		viperConfig := common.GetViperConfiguration(fullAppPath)
		configuration.EnableDynamicInstall = viperConfig.GetBool(consts.EnableDynamicInstallKey)
		configuration.PreRunCommand = viperConfig.GetString(consts.PreRunCommandEnvVarName)

		gen := RubyStartupScriptGenerator{
			SourcePath:         fullAppPath,
			UserStartupCommand: *userStartupCommandPtr,
			DefaultAppFilePath: fullDefaultAppFilePath,
			RailEnv:            *railEnvironment,
			BindPort:           *bindPortPtr,
			Manifest:           buildManifest,
			Configuration:      configuration,
		}
		script := gen.GenerateEntrypointScript()
		common.WriteScript(*outputPathPtr, script)

		userRunCommand := common.ParseUserRunCommand(filepath.Join(fullAppPath, consts.AppSvcFileName))
		common.AppendScript(*outputPathPtr, userRunCommand)
	}

	if setupEnvCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*setupEnvAppPathPtr)
		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)

		rubyInstallationRoot := fmt.Sprintf(
			"%s/%s",
			consts.RubyInstallationRootDir,
			buildManifest.RubyVersion)
		script := common.GetSetupScript(
			"ruby",
			buildManifest.RubyVersion,
			rubyInstallationRoot)
		scriptBuilder := strings.Builder{}
		scriptBuilder.WriteString(script)
		scriptBuilder.WriteString(
			fmt.Sprintf("export PATH=\"%s/bin:${PATH}\"\n", rubyInstallationRoot))
		finalScript := scriptBuilder.String()
		fmt.Println(fmt.Sprintf(
			"Setting up the environment with 'Ruby' version '%s'...\n",
			buildManifest.RubyVersion))
		common.SetupEnv(finalScript)
	}
}
