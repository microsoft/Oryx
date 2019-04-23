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
	StartupFileName string
}

const ManifestFileName = "oryx-manifest.toml"

func DeserializeBuildManifest(manifestFile string) BuildManifest {
    var manifest BuildManifest
    if _, err := toml.DecodeFile(manifestFile, &manifest); err != nil {
        log.Fatal(err)
    }
    return manifest
}

func GetBuildManifest(appPath string) BuildManifest {
    buildManifest := BuildManifest{}
    
    tomlFile := filepath.Join(appPath, ManifestFileName)
    if FileExists(tomlFile) {
        buildManifest = DeserializeBuildManifest(tomlFile)
    }
    return buildManifest
}