// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"encoding/json"
	"io/ioutil"
	"os"
	"path/filepath"
	"startupscriptgenerator/common"
	"strings"
)

type NodeStartupScriptGenerator struct {
	SourcePath                      string
	UserStartupCommand              string
	DefaultAppJsFilePath            string
	BindPort                        string
	CustomStartCommand              string
	RemoteDebugging                 bool
	RemoteDebuggingBreakBeforeStart bool
	RemoteDebuggingPort             string
	UseLegacyDebugger               bool //used for node versions < 7.7
	SkipNodeModulesExtraction       bool
}

type packageJson struct {
	Main    string
	Scripts *packageJsonScripts `json:"scripts"`
}

type packageJsonScripts struct {
	Start string `json:"start"`
}

const DefaultBindPort = "8080"
const NodeWrapperPath = "/opt/node-wrapper/"
const LocalIp = "0.0.0.0"
const inspectParamVariableName = "ORYX_NODE_INSPECT_PARAM"

func (gen *NodeStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("node.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown()

	logger.LogInformation("Generating script for source at '%s'", gen.SourcePath)

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("\n# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.SourcePath + "\n\n")

	// Expose the port so that a custom command can use it if needed.
	common.SetEnvironmentVariableInScript(&scriptBuilder, "PORT", gen.BindPort, DefaultBindPort)

	if !gen.SkipNodeModulesExtraction {
		const oryxManifestFile string = "oryx-manifest.toml"
		scriptBuilder.WriteString("if [ -f ./" + oryxManifestFile + " ]; then\n")
		scriptBuilder.WriteString("    echo \"Found '" + oryxManifestFile + "', checking if node_modules was compressed...\"\n")
		scriptBuilder.WriteString("    . ./" + oryxManifestFile + "\n")
		scriptBuilder.WriteString("    case $compressedNodeModulesFile in \n")
		scriptBuilder.WriteString("        *\".zip\")\n")
		scriptBuilder.WriteString("            echo \"Found zip-based node_modules.\"\n")
		scriptBuilder.WriteString("            extractionCommand=\"unzip -q $compressedNodeModulesFile -d /node_modules\"\n")
		scriptBuilder.WriteString("            ;;\n")
		scriptBuilder.WriteString("        *\".tar.gz\")\n")
		scriptBuilder.WriteString("            echo \"Found tar.gz based node_modules.\"\n")
		scriptBuilder.WriteString("            extractionCommand=\"tar -xzf $compressedNodeModulesFile -C /node_modules\"\n")
		scriptBuilder.WriteString("            ;;\n")
		scriptBuilder.WriteString("    esac\n")
		scriptBuilder.WriteString("    if [ ! -z \"$extractionCommand\" ]; then\n")
		scriptBuilder.WriteString("        echo \"Removing existing modules directory...\"\n")
		scriptBuilder.WriteString("        rm -fr /node_modules\n")
		scriptBuilder.WriteString("        mkdir -p /node_modules\n")
		scriptBuilder.WriteString("        echo \"Extracting modules...\"\n")
		scriptBuilder.WriteString("        $extractionCommand\n")
		// Some versions of node, in particular Node 4.8 and 6.2 according to our tests, do not find the node_modules
		// folder at the root. To handle these versions, we also add /node_modules to the NODE_PATH directory.
		scriptBuilder.WriteString("        export NODE_PATH=/node_modules:$NODE_PATH\n")
		scriptBuilder.WriteString("    fi\n")
		scriptBuilder.WriteString("    echo \"Done.\"\n")
		scriptBuilder.WriteString("fi\n\n")
	}

	// If user passed a custom startup command, it should take precedence above all other options
	startupCommand := strings.TrimSpace(gen.UserStartupCommand)
	commandSource := "User"
	if startupCommand == "" {
		logger.LogVerbose("No user-supplied startup command found")

		// deserialize package.json content
		packageJsonObj := getPackageJsonObject(gen.SourcePath)

		startupCommand = gen.getPackageJsonStartCommand(packageJsonObj)
		commandSource = "PackageJsonStart"

		if startupCommand == "" {
			logger.LogVerbose("scripts.start not found in package.json")
			if packageJsonObj != nil && packageJsonObj.Main != "" {
				logger.LogVerbose("Using startup command from package.json main field")
				startupCommand = gen.getStartupCommandFromJsFile(packageJsonObj.Main)
				commandSource = "PackageJsonMain"
			}
		}

		if startupCommand == "" {
			startupCommand = gen.getCandidateFilesStartCommand(gen.SourcePath)
			commandSource = "CandidateFile"
		}

		if startupCommand == "" {
			logger.LogWarning("Resorting to default startup command")
			startupCommand = gen.getDefaultAppStartCommand()
			commandSource = "DefaultApp"
		}

	} else {

		logger.LogInformation("adding execution permission if needed ...")
		isPermissionAdded := common.ParseCommandAndAddExecutionPermission(gen.UserStartupCommand, gen.SourcePath)
		logger.LogInformation("permission added %t", isPermissionAdded)
		logger.LogInformation("User-supplied startup command: '%s'", gen.UserStartupCommand)
		startupCommand = common.ExtendPathForCommand(startupCommand, gen.SourcePath)
	}

	scriptBuilder.WriteString(startupCommand + "\n")

	logger.LogProperties("Finalizing script", map[string]string{"commandSource": commandSource})

	return scriptBuilder.String()
}

// Gets the startup script from package.json if defined. Returns empty string if not found.
func (gen *NodeStartupScriptGenerator) getPackageJsonStartCommand(packageJsonObj *packageJson) string {
	if packageJsonObj != nil && packageJsonObj.Scripts != nil && packageJsonObj.Scripts.Start != "" {
		var commandBuilder strings.Builder
		if gen.RemoteDebugging || gen.RemoteDebuggingBreakBeforeStart {
			// At this point we know debugging is enabled, and node will be called indirectly through npm
			// or yarn. We inject the wrapper in the path so it executes instead of the node binary.
			commandBuilder.WriteString("export PATH=" + NodeWrapperPath + ":$PATH\n")
			debugFlag := gen.getDebugFlag()
			commandBuilder.WriteString("export " + inspectParamVariableName + "=\"" + debugFlag + "\"\n")
		}
		yarnLockPath := filepath.Join(gen.SourcePath, "yarn.lock") // TODO: consolidate with Microsoft.Oryx.BuildScriptGenerator.Node.NodeConstants.YarnLockFileName
		if common.FileExists(yarnLockPath) {
			commandBuilder.WriteString("yarn run start\n")
		} else {
			commandBuilder.WriteString("npm start --scripts-prepend-node-path false\n")
		}
		return commandBuilder.String()
	}
	return ""
}

// Try to find the main file for the app
func (gen *NodeStartupScriptGenerator) getCandidateFilesStartCommand(appPath string) string {
	logger := common.GetLogger("node.scriptgenerator.getCandidateFilesStartCommand")
	defer logger.Shutdown()

	startupFileCommand := ""
	filesToSearch := []string{"bin/www", "server.js", "app.js", "index.js", "hostingstart.js"}

	for _, file := range filesToSearch {
		fullPath := filepath.Join(gen.SourcePath, file)
		if common.FileExists(fullPath) {
			logger.LogInformation("Found startup candidate '%s'", fullPath)
			startupFileCommand = gen.getStartupCommandFromJsFile(fullPath)
			break
		}
	}

	return startupFileCommand
}

func (gen *NodeStartupScriptGenerator) getDefaultAppStartCommand() string {
	startupCommand := ""
	if gen.DefaultAppJsFilePath != "" {
		startupCommand = gen.getStartupCommandFromJsFile(gen.DefaultAppJsFilePath)
	}
	return startupCommand
}

func (gen *NodeStartupScriptGenerator) getStartupCommandFromJsFile(mainJsFilePath string) string {
	logger := common.GetLogger("node.scriptgenerator.getStartupCommandFromJsFile")
	defer logger.Shutdown()

	var commandBuilder strings.Builder
	if gen.RemoteDebugging || gen.RemoteDebuggingBreakBeforeStart {
		logger.LogInformation("Remote debugging on")
		debugFlag := gen.getDebugFlag()
		commandBuilder.WriteString("node " + debugFlag)
	} else if gen.CustomStartCommand != "" {
		commandBuilder.WriteString(gen.CustomStartCommand)
	} else {
		commandBuilder.WriteString("node")
	}

	commandBuilder.WriteString(" " + mainJsFilePath)
	return commandBuilder.String()
}

// Builds the flag that should be passed to `node` to enable debugging.
func (gen *NodeStartupScriptGenerator) getDebugFlag() string {
	var commandBuilder strings.Builder
	if gen.UseLegacyDebugger {
		commandBuilder.WriteString("--debug")
	} else {
		commandBuilder.WriteString("--inspect")
	}

	if gen.RemoteDebuggingBreakBeforeStart {
		commandBuilder.WriteString("-brk")
	}

	commandBuilder.WriteString("=" + LocalIp)
	if gen.RemoteDebuggingPort != "" {
		commandBuilder.WriteString(":" + gen.RemoteDebuggingPort)
	}

	return commandBuilder.String()
}

func getPackageJsonObject(appPath string) *packageJson {
	packageJsonPath := filepath.Join(appPath, "package.json")
	if _, err := os.Stat(packageJsonPath); !os.IsNotExist(err) {
		packageJsonBytes, err := ioutil.ReadFile(packageJsonPath)
		if err != nil {
			return nil
		}

		packageJsonObj := new(packageJson)
		err = json.Unmarshal(packageJsonBytes, &packageJsonObj)
		if err == nil {
			return packageJsonObj
		}
	}
	return nil
}
