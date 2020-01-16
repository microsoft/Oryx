// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"flag"
	"fmt"
	"os"
)

var ManifestDirFlag = flag.String(
	"manifestDir",
	"",
	"[Optional] Path to the directory where build manifest file can be found. If no value is provided, then "+
		"it is assumed to be under the directory specified by 'appPath'.")

const SetupEnvCommandName string = "setupEnv"
const ScriptCommandName string = "script"

func ValidateCommands(scriptCommand *flag.FlagSet, setupEnvCommand *flag.FlagSet) {
	// Verify that a subcommand has been provided
	// os.Arg[0] is the main command
	// os.Arg[1] will be the subcommand
	if len(os.Args) < 2 {
		fmt.Println(fmt.Sprintf("Error: '%s' or '%s' subcommand is required", SetupEnvCommandName, ScriptCommandName))
		os.Exit(1)
	}

	// Switch on the subcommand
	// Parse the flags for appropriate FlagSet
	// FlagSet.Parse() requires a set of arguments to parse as input
	// os.Args[2:] will be all arguments starting after the subcommand at os.Args[1]
	switch os.Args[1] {
	case ScriptCommandName:
		scriptCommand.Parse(os.Args[2:])
	case SetupEnvCommandName:
		setupEnvCommand.Parse(os.Args[2:])
	default:
		flag.PrintDefaults()
		os.Exit(1)
	}
}
