// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"bufio"
	"fmt"
	"os"
	"strings"
)

func ParseUserRunCommand(sourcePath string) string {

	appsvcFile, err := os.Open(sourcePath)
	if err != nil {
		return ""
	}
	defer appsvcFile.Close()

	const runHeading string = "run:" // format of run command- "run: gunicorn myapp.app --workers 5"
	runCommand := ""
	var isRunCommandFound bool
	scanner := bufio.NewScanner(appsvcFile)

	for scanner.Scan() {
		indexOfRunHeading := strings.Index(scanner.Text(), runHeading)
		isCurrentLineContainsAnyHeading := strings.Contains(scanner.Text(), ":")

		if isCurrentLineContainsAnyHeading && isRunCommandFound {
			// runCommand already found
			// not considering any other customized commands
			break
		}

		isValidRunCommand := indexOfRunHeading == 0 && len(scanner.Text()) > len(runHeading)
		if isRunCommandFound || isValidRunCommand {
			if isRunCommandFound {
				runCommand += "\n"
				runCommand += strings.TrimSpace(scanner.Text())
			} else {
				isRunCommandFound = true
				runCommand += strings.TrimSpace(scanner.Text()[indexOfRunHeading+len(runHeading):]) //gets the run command and trim to get rid of the forward and trailing spaces
			}
		}
	}

	fmt.Println("User provided run command: " + runCommand)

	if err := scanner.Err(); err != nil {
		fmt.Println(err)
		return ""
	}

	return runCommand
}
