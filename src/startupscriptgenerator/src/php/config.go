// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

type Configuration struct {
	PhpOrigin          string
	PreRunCommand      string
	FpmMaxChildren     string
	FpmStartServers    string
	FpmMinSpareServers string
	FpmMaxSpareServers string
	NginxConfFile      string
}
