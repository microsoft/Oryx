// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
    "github.com/BurntSushi/toml"
    "log"
    "path/filepath"
)

type BuildManifest struct {
	ZipAllOutput string
	CompressedNodeModulesFile string
}

func DeserializeBuildManifest(manifestFile string) BuildManifest {
    var manifest BuildManifest
    if _, err := toml.DecodeFile(manifestFile, &manifest); err != nil {
        log.Fatal(err)
    }
    return manifest
}

func GetBuildManifest(appPath string) BuildManifest {
    buildManifest := BuildManifest{
		ZipAllOutput: "",
		CompressedNodeModulesFile: "",
    }
    
    tomlFile := filepath.Join(appPath, "oryx-manifest.toml")
	if FileExists(tomlFile) {
		buildManifest = DeserializeBuildManifest(tomlFile)
	}
    return buildManifest
}