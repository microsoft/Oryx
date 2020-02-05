// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"common/consts"
	"flag"
	"fmt"
	"os"
)

func ValidateCommands(versionCommand *flag.FlagSet, scriptCommand *flag.FlagSet, setupEnvCommand *flag.FlagSet) {
	// Verify that a subcommand has been provided
	// os.Arg[0] is the main command
	// os.Arg[1] will be the subcommand
	if len(os.Args) < 2 {
		fmt.Println(fmt.Sprintf(
			"Error: '%s' or '%s' or '%s' subcommand is required",
			consts.VersionCommandName,
			consts.SetupEnvCommandName,
			consts.CreateScriptCommandName))
		os.Exit(1)
	}

	// Switch on the subcommand
	// Parse the flags for appropriate FlagSet
	// FlagSet.Parse() requires a set of arguments to parse as input
	// os.Args[2:] will be all arguments starting after the subcommand at os.Args[1]
	switch os.Args[1] {
	case consts.VersionCommandName:
		PrintVersionInfo()
	case consts.CreateScriptCommandName:
		scriptCommand.Parse(os.Args[2:])
	case consts.SetupEnvCommandName:
		setupEnvCommand.Parse(os.Args[2:])
	default:
		flag.PrintDefaults()
		os.Exit(1)
	}
}
