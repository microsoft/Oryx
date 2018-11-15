package main

import (
	"encoding/json"
	"io/ioutil"
	"os"
	"path/filepath"
	"strings"
)

type NodeStartupScriptGenerator struct {
	SourcePath                      string
	DefaultAppJsFilePath            string
	CustomStartCommand              string
	RemoteDebugging                 bool
	RemoteDebuggingBreakBeforeStart bool
	RemoteDebuggingIp               string
	RemoteDebuggingPort             string
	UseLegacyDebugger               bool //used for node versions < 7.7
}

type packageJson struct {
	Scripts *packageJsonScripts `json:"scripts"`
}

type packageJsonScripts struct {
	Start string `json:"start"`
}

func (gen *NodeStartupScriptGenerator) GenerateEntrypointCommand() string {
	startupCommand := getPackageJsonStartCommand(gen.SourcePath)
	if startupCommand == "" {
		startupCommand = gen.getCandidateFilesStartCommand()
	}
	if startupCommand == "" {
		startupCommand = gen.getDefaultAppStartCommand()
	}
	return startupCommand
}

// Gets the startup script from package.json if defined. Returns empty string if not found.
func getPackageJsonStartCommand(appPath string) string {
	packageJsonPath := filepath.Join(appPath, "package.json")
	if _, err := os.Stat(packageJsonPath); !os.IsNotExist(err) {
		packageJsonBytes, err := ioutil.ReadFile(packageJsonPath)
		if err == nil {
			packageObj := &packageJson{
				Scripts: &packageJsonScripts{
					Start: "",
				},
			}
			err := json.Unmarshal(packageJsonBytes, &packageObj)
			if err == nil && packageObj.Scripts.Start != "" {
				return "npm start"
			}
		}
	}
	return ""
}

// Try to find the main file for the app
func (gen *NodeStartupScriptGenerator) getCandidateFilesStartCommand() string {
	startupFileCommand := ""
	filesToSearch := []string{"bin/www", "server.js", "app.js", "index.js", "hostingstart.js"}
	for _, file := range filesToSearch {
		fullPath := filepath.Join(gen.SourcePath, file)
		if _, err := os.Stat(fullPath); !os.IsNotExist(err) {
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
	var commandBuilder strings.Builder
	if gen.RemoteDebugging || gen.RemoteDebuggingBreakBeforeStart {

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
