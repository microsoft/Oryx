// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"fmt"
)

func ExamplePythonStartupScriptGenerator_getCommandFromModule_onlyModule() {
	// Arrange
	gen := PythonStartupScriptGenerator{
		BindPort: "",
	}
	// Act
	command := gen.getCommandFromModule("module.py", "")
	fmt.Println(command)
	// Output:
	// GUNICORN_CMD_ARGS="--timeout 600 --access-logfile '-' --error-logfile '-'" gunicorn module.py
}

func ExamplePythonStartupScriptGenerator_getCommandFromModule_moduleAndPath() {
	// Arrange
	gen := PythonStartupScriptGenerator{
		BindPort: "",
	}
	// Act
	command := gen.getCommandFromModule("module.py", "/a/b/c")
	fmt.Println(command)
	// Output:
	// GUNICORN_CMD_ARGS="--timeout 600 --access-logfile '-' --error-logfile '-' --chdir=/a/b/c" gunicorn module.py
}

func ExamplePythonStartupScriptGenerator_getCommandFromModule_moduleAndPathAndHost() {
	// Arrange
	gen := PythonStartupScriptGenerator{
		BindPort: "12345",
	}
	// Act
	command := gen.getCommandFromModule("module.py", "/a/b/c")
	fmt.Println(command)
	// Output:
	// GUNICORN_CMD_ARGS="--timeout 600 --access-logfile '-' --error-logfile '-' --bind=0.0.0.0:12345 --chdir=/a/b/c" gunicorn module.py
}
