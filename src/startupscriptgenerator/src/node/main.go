// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"common"
	"flag"
	"fmt"
	"os"
	"strconv"
	"strings"
)

const dependencyInstallationScript string = "/tmp/oryx/images/runtime/node/installDependencies.sh"

func main() {
	common.PrintVersionInfo()

	// Subcommands
	setupEnvCommand := flag.NewFlagSet(consts.SetupEnvCommandName, flag.ExitOnError)
	setupEnvAppPathPtr := setupEnvCommand.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")

	scriptCommand := flag.NewFlagSet(consts.ScriptCommandName, flag.ExitOnError)
	appPathPtr := scriptCommand.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	manifestDirPtr := common.ManifestDirFlag
	userStartupCommandPtr := scriptCommand.String("userStartupCommand", "", "[Optional] Command that will be executed to start the application up.")
	defaultAppFilePathPtr := scriptCommand.String("defaultApp", "", "[Optional] Path to a default file that will be executed if the entrypoint is not found. Ex: '/opt/startup/default-static-site.js'")
	bindPortPtr := scriptCommand.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 8080")
	usePm2Ptr := scriptCommand.Bool("usePM2", false, "If enabled, application will run using PM2.")
	remoteDebugEnabledPtr := scriptCommand.Bool("remoteDebug", false, "Application will run in debug mode.")
	remoteDebugBrkEnabledPtr := scriptCommand.Bool("remoteDebugBrk", false, "Application will run in debug mode, and will debugger will break before the user code starts.")
	remoteDebugPort := scriptCommand.String("debugPort", "", "The port the debugger will listen to.")
	outputPathPtr := scriptCommand.String("output", "run.sh", "Path to the script to be generated.")
	skipNodeModulesExtraction := scriptCommand.Bool(
		"skipNodeModulesExtraction",
		false,
		"Disables the extraction of node_modules file. If used, some external tool will have to extract it - "+
			"otherwise the application might not work.")
	setEnvironment := scriptCommand.Bool(
		consts.SetupEnvCommandName,
		false,
		"If true, adds content to setup the environment before running the app")

	logger := common.GetLogger("node.main")
	defer logger.Shutdown()
	logger.StartupScriptRequested()

	common.ValidateCommands(scriptCommand, setupEnvCommand)

	if scriptCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*appPathPtr)
		defaultAppFullPAth := common.GetValidatedFullPath(*defaultAppFilePathPtr)
		useLegacyDebugger := isLegacyDebuggerNeeded()

		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
		common.SetGlobalOperationID(buildManifest)

		gen := NodeStartupScriptGenerator{
			SourcePath:                      fullAppPath,
			UserStartupCommand:              *userStartupCommandPtr,
			DefaultAppJsFilePath:            defaultAppFullPAth,
			UsePm2:                          *usePm2Ptr,
			BindPort:                        *bindPortPtr,
			RemoteDebugging:                 *remoteDebugEnabledPtr,
			RemoteDebuggingBreakBeforeStart: *remoteDebugBrkEnabledPtr,
			RemoteDebuggingPort:             *remoteDebugPort,
			UseLegacyDebugger:               useLegacyDebugger,
			SkipNodeModulesExtraction:       *skipNodeModulesExtraction,
			Manifest:                        buildManifest,
			SetupEnvironment:                *setEnvironment,
		}
		script := gen.GenerateEntrypointScript()
		common.WriteScript(*outputPathPtr, script)
	}

	if setupEnvCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*setupEnvAppPathPtr)
		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)
		script := common.GetSetupScript(
			"nodejs",
			buildManifest.NodeVersion,
			consts.NodeInstallationDir)
		scriptBuilder := strings.Builder{}
		scriptBuilder.WriteString(script)
		scriptBuilder.WriteString(fmt.Sprintf("export PATH=\"%s/bin:${PATH}\"\n", consts.NodeInstallationDir))
		scriptBuilder.WriteString("echo Installing dependencies...\n")
		scriptBuilder.WriteString(fmt.Sprintf("%s\n", dependencyInstallationScript))
		fmt.Println(fmt.Sprintf(
			"Setting up the environment with 'node' version '%s'...\n",
			buildManifest.NodeVersion))
		finalScript := scriptBuilder.String()
		common.SetupEnv(finalScript)
	}
}

// Checks if the legacy debugger should be used for the current node image
func isLegacyDebuggerNeeded() bool {
	nodeVersionEnv := os.Getenv("NODE_VERSION")
	result := checkLegacyDebugger(nodeVersionEnv)
	return result
}

// Checks if the legacy debugger should be used for a particular node version
func checkLegacyDebugger(nodeVersion string) bool {
	if nodeVersion != "" {
		if splitVersion := strings.Split(nodeVersion, "."); len(splitVersion) >= 2 {
			if majorVersion, majorErr := strconv.ParseInt(splitVersion[0], 0, 0); majorErr == nil {
				if minorVersion, minorErr := strconv.ParseInt(splitVersion[1], 0, 0); minorErr == nil {
					if majorVersion < 7 || (majorVersion == 7 && minorVersion < 7) {
						return true
					}
				}
			}
		}
	}
	return false
}
