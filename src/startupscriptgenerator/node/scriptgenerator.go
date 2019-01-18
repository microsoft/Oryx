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
	CustomStartCommand              string
	RemoteDebugging                 bool
	RemoteDebuggingBreakBeforeStart bool
	RemoteDebuggingIp               string
	RemoteDebuggingPort             string
	UseLegacyDebugger               bool //used for node versions < 7.7
}

type packageJson struct {
	Main    string
	Scripts *packageJsonScripts `json:"scripts"`
}

type packageJsonScripts struct {
	Start string `json:"start"`
}

func (gen *NodeStartupScriptGenerator) GenerateEntrypointScript() string {
	logger := common.GetLogger("node.scriptgenerator.GenerateEntrypointScript")
	defer logger.Shutdown() // Not shutting down other loggers to avoid too-long hangs

	logger.LogInformation("Generating script for source at '%s'", gen.SourcePath)

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("\n# Enter the source directory to make sure the script runs where the user expects\n")
	scriptBuilder.WriteString("cd " + gen.SourcePath + "\n")

	commandSource := ""

	// If user passed a custom startup command, it should take precedence above all other options
	startupCommand := strings.TrimSpace(gen.UserStartupCommand)
	if startupCommand == "" {
		logger.LogVerbose("No user-supplied startup command found")

		// deserialize package.json content
		packageJsonObj := getPackageJsonObject(gen.SourcePath)

		startupCommand = getPackageJsonStartCommand(packageJsonObj)
		if startupCommand != "" {
			commandSource = "PackageJsonStart"
		} else {
			logger.LogVerbose("scripts.start not found in package.json")
			if packageJsonObj != nil && packageJsonObj.Main != "" {
				logger.LogVerbose("Using startup command from package.json main field")
				startupCommand = gen.getStartupCommandFromJsFile(packageJsonObj.Main)
			}
		}

		if startupCommand != "" {
			commandSource = "PackageJsonMain"
		} else {
			startupCommand = gen.getCandidateFilesStartCommand(gen.SourcePath)
		}

		if startupCommand != "" {
			commandSource = "CandidateFile"
		} else {
			logger.LogWarning("Resorting to default startup command")
			startupCommand = gen.getDefaultAppStartCommand()
			commandSource = "DefaultApp"
		}
	} else {
		commandSource = "User"
		logger.LogInformation("User-supplied startup command: '%s'", gen.UserStartupCommand)
	}

	scriptBuilder.WriteString(startupCommand + "\n")

	logger.LogProperties("Finalizing script", map[string]string{"commandSource": commandSource})

	return scriptBuilder.String()
}

// Gets the startup script from package.json if defined. Returns empty string if not found.
func getPackageJsonStartCommand(packageJsonObj *packageJson) string {
	if packageJsonObj != nil && packageJsonObj.Scripts != nil && packageJsonObj.Scripts.Start != "" {
		return "npm start"
	}
	return ""
}

// Try to find the main file for the app
func (gen *NodeStartupScriptGenerator) getCandidateFilesStartCommand(appPath string) string {
	logger := common.GetLogger("node.scriptgenerator.getCandidateFilesStartCommand")

	startupFileCommand := ""
	filesToSearch := []string{"bin/www", "server.js", "app.js", "index.js", "hostingstart.js"}

	for _, file := range filesToSearch {
		fullPath := filepath.Join(gen.SourcePath, file)
		if _, err := os.Stat(fullPath); !os.IsNotExist(err) {
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

	var commandBuilder strings.Builder
	if gen.RemoteDebugging || gen.RemoteDebuggingBreakBeforeStart {
		logger.LogInformation("Remote debugging on")

		if gen.UseLegacyDebugger {
			commandBuilder.WriteString("node --debug")
		} else {
			commandBuilder.WriteString("node --inspect")
		}

		if gen.RemoteDebuggingBreakBeforeStart {
			commandBuilder.WriteString("-brk")
		}

		if gen.RemoteDebuggingIp != "" {
			commandBuilder.WriteString("=" + gen.RemoteDebuggingIp)
			if gen.RemoteDebuggingPort != "" {
				commandBuilder.WriteString(":" + gen.RemoteDebuggingPort)
			}
		}
	} else if gen.CustomStartCommand != "" {
		commandBuilder.WriteString(gen.CustomStartCommand)
	} else {
		commandBuilder.WriteString("node")
	}

	commandBuilder.WriteString(" " + mainJsFilePath)
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
