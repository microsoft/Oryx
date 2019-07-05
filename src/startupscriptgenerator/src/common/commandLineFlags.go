// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import "flag"

var ManifestDirFlag = flag.String(
	"manifestDir",
	"",
	"[Optional] Path to the directory where build manifest file can be found. If no value is provided, then "+
		"it is assumed to be under the directory specified by 'appPath'.")
