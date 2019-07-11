// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"runtime"
	"strconv"
	"testing"

	"github.com/stretchr/testify/assert"
)

func Test_ExamplePythonStartupScriptGenerator_getCommandFromModule_onlyModule(t *testing.T) {
	// Arrange
	workerCount := getWorkerCount()
	expected := "GUNICORN_CMD_ARGS=\"--timeout 600 --access-logfile '-' --error-logfile '-' --workers=" +
		workerCount + "\" gunicorn module.py"
	gen := PythonStartupScriptGenerator{
		BindPort: "",
	}

	// Act
	actual := gen.getCommandFromModule("module.py", "")

	// Assert
	assert.Equal(t, expected, actual)
}

func ExamplePythonStartupScriptGenerator_getCommandFromModule_moduleAndPath(t *testing.T) {
	// Arrange
	workerCount := getWorkerCount()
	expected := "GUNICORN_CMD_ARGS=\"--timeout 600 --access-logfile '-' --error-logfile '-' --workders=" +
		workerCount + " --chdir=/a/b/c\" gunicorn module.py"
	gen := PythonStartupScriptGenerator{
		BindPort: "",
	}

	// Act
	actual := gen.getCommandFromModule("module.py", "/a/b/c")

	// Assert
	assert.Equal(t, expected, actual)
}

func ExamplePythonStartupScriptGenerator_getCommandFromModule_moduleAndPathAndHost(t *testing.T) {
	// Arrange
	workerCount := getWorkerCount()
	expected := "GUNICORN_CMD_ARGS=\"--timeout 600 --access-logfile '-' --error-logfile '-' --workers=" +
		workerCount + "--bind=0.0.0.0:12345 --chdir=/a/b/c\" gunicorn module.py"
	gen := PythonStartupScriptGenerator{
		BindPort: "12345",
	}

	// Act
	actual := gen.getCommandFromModule("module.py", "/a/b/c")

	// Assert
	assert.Equal(t, expected, actual)
}

func Test_GetsWorkerCountBasedOnNumberOfCores(t *testing.T) {
	// Arrange
	cpuCount := runtime.NumCPU()
	expected := strconv.Itoa((2 * cpuCount) + 1)

	// Act
	actual := getWorkerCount()

	// Assert
	assert.Equal(t, expected, actual)
}
