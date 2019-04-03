// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"bytes"
	"fmt"
	"os/exec"
	"strings"
)

func ExtractZippedOutput(srcFolder string, destFolder string) {
	zipFileName := "oryx_output.tar.gz"
	scriptPath := "/tmp/test.sh"
	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("set -e\n\n")
	scriptBuilder.WriteString("if [ -d \"" + destFolder + "\" ]; then\n")
	scriptBuilder.WriteString("    rm -rf \"" + destFolder + "\"\n")
	scriptBuilder.WriteString("fi\n")
	scriptBuilder.WriteString("mkdir -p \"" + destFolder + "\"\n")
	scriptBuilder.WriteString("cp -rf \"" + srcFolder + "\"/* \"" + destFolder + "\"\n")
	scriptBuilder.WriteString("cd \"" + destFolder + "\"\n")
	scriptBuilder.WriteString("if [ -f \"" + zipFileName + "\" ]; then\n")
	scriptBuilder.WriteString("    echo \"Found '" + zipFileName + "', will extract its contents.\"\n")
	scriptBuilder.WriteString("    echo \"Extracting...\"\n")
	scriptBuilder.WriteString("    tar -xzf " + zipFileName + "\n")
	scriptBuilder.WriteString("    echo \"Done.\"\n")
	scriptBuilder.WriteString("fi\n\n")

	WriteScript(scriptPath, scriptBuilder.String())
	ExecuteCommand("/bin/sh", "-c", scriptPath)
}

func CopyOutputToIntermediateDir(srcFolder string, destFolder string) {
	scriptPath := "/tmp/test.sh"
	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("set -e\n\n")
	scriptBuilder.WriteString("if [ -d \"" + destFolder + "\" ]; then\n")
	scriptBuilder.WriteString("    rm -rf \"" + destFolder + "\"\n")
	scriptBuilder.WriteString("fi\n")
	scriptBuilder.WriteString("mkdir -p \"" + destFolder + "\"\n")
	scriptBuilder.WriteString("cp -rf \"" + srcFolder + "\"/* \"" + destFolder + "\"\n")

	WriteScript(scriptPath, scriptBuilder.String())
	ExecuteCommand("/bin/sh", "-c", scriptPath)
}

func ExecuteCommand(name string, arg ...string) {
	command := exec.Command(name, arg...)
	var out bytes.Buffer
	var stderr bytes.Buffer
	command.Stdout = &out
	command.Stderr = &stderr
	err := command.Run()
	if err != nil {
		fmt.Println(fmt.Sprint(err) + ": " + stderr.String())
		panic(err)
	}
	fmt.Println("Result: " + out.String())
}