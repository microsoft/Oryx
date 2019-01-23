// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

// NOTE: The 'BuildNumber' and 'Commit' variables here are modified during build time. Look at
// the 'go build' command in file 'startupscriptgenerator/build.sh'
var (
	BuildNumber = "unspecified"
	Version     = "0.2" + "." + BuildNumber
	Commit      = "unspecified"
)

func PrintVersionInfo() {
	println("Oryx Version : " + Version + ", Commit: " + Commit + "\n")
}
