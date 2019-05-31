// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
    "startupscriptgenerator/common/consts"
    "github.com/BurntSushi/toml"
    "log"
    "path/filepath"
)

type BuildManifest struct {
    StartupFileName     string
    ZipAllOutput        string
    OperationId         string
}

var _buildManifest = BuildManifest{}
var _readManifest = false

func DeserializeBuildManifest(manifestFile string) BuildManifest {
    var manifest BuildManifest
    if _, err := toml.DecodeFile(manifestFile, &manifest); err != nil {
        log.Fatal(err)
    }
    return manifest
}

func GetBuildManifest(appPath string) BuildManifest {
    if _readManifest {
        return _buildManifest
    }
    
    tomlFile := filepath.Join(appPath, consts.BuildManifestFileName)
    if FileExists(tomlFile) {
        _buildManifest = DeserializeBuildManifest(tomlFile)
        _readManifest = true
    }

    return _buildManifest
}