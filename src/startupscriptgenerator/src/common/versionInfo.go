// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"fmt"
)

// NOTE: The 'BuildNumber' and 'Commit' variables here are modified during build time. Look at
// the 'go build' command in file 'startupscriptgenerator/build.sh'
var (
	BuildNumber    = "unspecified"
	Version        = "0.2" + "." + BuildNumber
	Commit         = "unspecified"
	ReleaseTagName = "unspecified"
)

func PrintVersionInfo() {
	fmt.Printf("Oryx Version: %s, Commit: %s, ReleaseTagName: %s\n", Version, Commit, ReleaseTagName)
}
