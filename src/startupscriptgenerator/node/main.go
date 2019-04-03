// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"flag"
	"os"
	"startupscriptgenerator/common"
	"strconv"
	"strings"
)

func main() {
	common.PrintVersionInfo()

	appPathPtr := flag.String("appPath", ".", "The path to the application folder, e.g. '/home/site/wwwroot/'.")
	userStartupCommandPtr := flag.String("userStartupCommand", "", "[Optional] Command that will be executed to start the application up.")
	defaultAppFilePathPtr := flag.String("defaultApp", "", "[Optional] Path to a default file that will be executed if the entrypoint is not found. Ex: '/opt/startup/default-static-site.js'")
	bindPortPtr := flag.String("bindPort", "", "[Optional] Port where the application will bind to. Default is 8080")
	customStartCommandPtr := flag.String("serverCmd", "", "[Optional] Command to start the server, if different than 'node', e.g. 'pm2 start --no-daemon'")
	remoteDebugEnabledPtr := flag.Bool("remoteDebug", false, "Application will run in debug mode.")
	remoteDebugBrkEnabledPtr := flag.Bool("remoteDebugBrk", false, "Application will run in debug mode, and will debugger will break before the user code starts.")
	remoteDebugIp := flag.String("debugHost", "", "The IP address where the debugger will listen to, e.g. '0.0.0.0' or '127.0.0.1")
	remoteDebugPort := flag.String("debugPort", "", "The port the debugger will listen to.")
	outputPathPtr := flag.String("output", "run.sh", "Path to the script to be generated.")
	skipNodeModulesExtraction := flag.Bool(
		"skipNodeModulesExtraction",
		false,
		"Disables the extraction of node_modules file. If used, some external tool will have to extract it - "+
			"otherwise the application might not work.")
	flag.Parse()

	fullAppPath := common.GetValidatedFullPath(*appPathPtr)
	defaultAppFullPAth := common.GetValidatedFullPath(*defaultAppFilePathPtr)
	useLegacyDebugger := isLegacyDebuggerNeeded()

	common.SetGlobalOperationId(fullAppPath)

	buildManifest := common.GetBuildManifest(fullAppPath)
	if buildManifest.ZipAllOutput == "true" {
		srcFolder := fullAppPath
		destFolder := "/tmp/output"

		common.ExtractZippedOutput(srcFolder, destFolder)

		// Update the variables so the downstream code uses these updated paths
		fullAppPath = destFolder
	}

	gen := NodeStartupScriptGenerator{
		SourcePath:                      fullAppPath,
		UserStartupCommand:              *userStartupCommandPtr,
		DefaultAppJsFilePath:            defaultAppFullPAth,
		CustomStartCommand:              *customStartCommandPtr,
		BindPort:                        *bindPortPtr,
		RemoteDebugging:                 *remoteDebugEnabledPtr,
		RemoteDebuggingBreakBeforeStart: *remoteDebugBrkEnabledPtr,
		RemoteDebuggingIp:               *remoteDebugIp,
		RemoteDebuggingPort:             *remoteDebugPort,
		UseLegacyDebugger:               useLegacyDebugger,
		SkipNodeModulesExtraction:       *skipNodeModulesExtraction,
	}
	script := gen.GenerateEntrypointScript()
	common.WriteScript(*outputPathPtr, script)
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
