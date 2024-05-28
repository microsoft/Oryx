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
	"strings"
)

func getCommandNameList(commands []*flag.FlagSet) []string {
	cmdsList := make([]string, len(commands))
	for i := 0; i < len(cmdsList); i++ {
		cmdsList[i] = commands[i].Name()
	}
	return cmdsList
}

func ValidateCommands(commands []*flag.FlagSet) {
	// Verify that a subcommand has been provided
	// os.Arg[0] is the main command (ex: 'oryx')
	// os.Arg[1] will be the subcommand (ex: 'create-script')
	if len(os.Args) < 2 {
		cmdsList := getCommandNameList(commands)
		fmt.Println(fmt.Sprintf(
			"Error: Invalid execution. Available subcommands: %s",
			strings.Join(cmdsList, ", ")))
		os.Exit(1)
	}

	// Switch on the subcommand
	// Parse the flags for appropriate FlagSet
	// FlagSet.Parse() requires a set of arguments to parse as input
	// os.Args[2:] will be all arguments starting after the subcommand at os.Args[1]
	suppliedCommandName := os.Args[1]
	if suppliedCommandName == consts.VersionCommandName {
		PrintVersionInfo()
	} else {
		isValidCommand := false
		for i := 0; i < len(commands); i++ {
			if suppliedCommandName == commands[i].Name() {
				isValidCommand = true
				commands[i].Parse(os.Args[2:])
				break
			}
		}
		if !isValidCommand {
			cmdsList := getCommandNameList(commands)
			fmt.Println(fmt.Sprintf(
				"Error: Invalid subcommand '%s'. Available subcommands: %s",
				suppliedCommandName,
				strings.Join(cmdsList, ", ")))
			os.Exit(1)
		}
	}
}
