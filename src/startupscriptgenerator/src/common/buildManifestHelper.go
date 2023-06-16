// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"common/consts"
	"fmt"
	"os"
	"path/filepath"

	"github.com/BurntSushi/toml"
)

type BuildManifest struct {
	StartupFileName                 string
	OperationID                     string
	VirtualEnvName                  string
	PackageDir                      string
	CompressedVirtualEnvFile        string
	StartupDllFileName              string
	InjectedAppInsights             string
	CompressedNodeModulesFile       string
	DotNetCoreSdkVersion            string
	DotNetCoreRuntimeVersion        string
	NodeVersion                     string
	PythonVersion                   string
	RubyVersion                     string
	GolangVersion                   string
	SourceDirectoryInBuildContainer string
	CompressDestinationDir          string
}

var _buildManifest BuildManifest
var _hasResult = false

func GetBuildManifest(manifestDir *string, fullAppPath string) BuildManifest {
	if _hasResult {
		return _buildManifest
	}

	manifestFileFullPath := getManifestFile(manifestDir, fullAppPath)
	if FileExists(manifestFileFullPath) {
		fmt.Printf("Found build manifest file at '%s'. Deserializing it...\n", manifestFileFullPath)
		_buildManifest = deserializeBuildManifest(manifestFileFullPath)
	} else {
		fmt.Printf("Could not find build manifest file at '%s'\n", manifestFileFullPath)
	}

	_hasResult = true
	return _buildManifest
}

func getManifestFile(manifestDir *string, fullAppPath string) string {
	manifestFileFullPath := ""
	if *manifestDir == "" {
		manifestFileFullPath = filepath.Join(fullAppPath, consts.BuildManifestFileName)
	} else {
		providedPath := *manifestDir
		absPath, err := filepath.Abs(providedPath)
		if err != nil || !PathExists(absPath) {
			fmt.Printf(
				"Error: Provided manifest file directory path '%s' is not valid or does not exist.\n",
				providedPath)
			os.Exit(consts.FAILURE_EXIT_CODE)
		}

		manifestFileFullPath = filepath.Join(absPath, consts.BuildManifestFileName)
		if !FileExists(manifestFileFullPath) {
			fmt.Printf("Error: Could not file manifest file '%s' at '%s'.\n", consts.BuildManifestFileName, absPath)
			os.Exit(consts.FAILURE_EXIT_CODE)
		}
	}
	return manifestFileFullPath
}

func deserializeBuildManifest(manifestFile string) BuildManifest {
	var manifest BuildManifest
	if _, err := toml.DecodeFile(manifestFile, &manifest); err != nil {
		fmt.Printf(
			"Error occurred when trying to deserialize the manifest file '%s'. Error: '%s'.\n",
			manifestFile,
			err)
		os.Exit(consts.FAILURE_EXIT_CODE)
	}
	return manifest
}
