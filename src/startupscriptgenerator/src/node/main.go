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
	"os"
	"strconv"
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
	usePm2Ptr := scriptCommand.Bool("usePM2", false, "If enabled, application will run using PM2.")
	remoteDebugEnabledPtr := scriptCommand.Bool("remoteDebug", false, "Application will run in debug mode.")
	remoteDebugBrkEnabledPtr := scriptCommand.Bool(
		"remoteDebugBrk",
		false,
		"Application will run in debug mode, and will debugger will break before the user code starts.")
	remoteDebugPort := scriptCommand.String("debugPort", "", "The port the debugger will listen to.")
	outputPathPtr := scriptCommand.String("output", "run.sh", "Path to the script to be generated.")
	skipNodeModulesExtraction := scriptCommand.Bool(
		"skipNodeModulesExtraction",
		false,
		"Disables the extraction of node_modules file. If used, some external tool will have to extract it - "+
			"otherwise the application might not work.")
	enableDynamicInstall := scriptCommand.Bool(
		"enableDynamicInstall",
		false,
		"Enables installing SDK dynamically if not present in the container already. Default if false.")
	flag.Parse()

	logger := common.GetLogger("node.main")
	defer logger.Shutdown()
	logger.StartupScriptRequested()

	common.ValidateCommands(versionCommand, scriptCommand, setupEnvCommand)

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
			EnableDynamicInstall:            *enableDynamicInstall,
			Manifest:                        buildManifest,
		}
		script := gen.GenerateEntrypointScript()
		common.WriteScript(*outputPathPtr, script)
	}

	if setupEnvCommand.Parsed() {
		fullAppPath := common.GetValidatedFullPath(*setupEnvAppPathPtr)
		buildManifest := common.GetBuildManifest(manifestDirPtr, fullAppPath)

		nodeInstallationRoot := "/usr/local"
		script := common.GetSetupScript(
			"nodejs",
			buildManifest.NodeVersion,
			nodeInstallationRoot)
		scriptBuilder := strings.Builder{}
		scriptBuilder.WriteString(script)
		scriptBuilder.WriteString("echo Installing dependencies...\n")
		scriptBuilder.WriteString("/opt/node/installDependencies.sh\n")
		finalScript := scriptBuilder.String()
		fmt.Println(fmt.Sprintf(
			"Setting up the environment with 'NodeJS' version '%s'...\n",
			buildManifest.PythonVersion))
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
