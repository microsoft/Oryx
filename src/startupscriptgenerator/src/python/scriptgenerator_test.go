// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"testing"
	"github.com/stretchr/testify/assert"
)

func Test_ExamplePythonStartupScriptGenerator_buildGunicornCommandForModule_onlyModule(t *testing.T) {
	// Arrange
	expected := "GUNICORN_CMD_ARGS=\"--timeout 600 --access-logfile '-' --error-logfile '-'" +
		"\" gunicorn module.py"
	gen := PythonStartupScriptGenerator{
		BindPort: "",
	}

	// Act
	actual := gen.buildGunicornCommandForModule("module.py", "")

	// Assert
	assert.Equal(t, expected, actual)
}

func ExamplePythonStartupScriptGenerator_buildGunicornCommandForModule_moduleAndPath(t *testing.T) {
	// Arrange
	expected := "GUNICORN_CMD_ARGS=\"--timeout 600 --access-logfile '-' --error-logfile '-'" +
		" --chdir=/a/b/c\" gunicorn module.py"
	gen := PythonStartupScriptGenerator{
		BindPort: "",
	}

	// Act
	actual := gen.buildGunicornCommandForModule("module.py", "/a/b/c")

	// Assert
	assert.Equal(t, expected, actual)
}

func ExamplePythonStartupScriptGenerator_buildGunicornCommandForModule_moduleAndPathAndHost(t *testing.T) {
	// Arrange
	expected := "GUNICORN_CMD_ARGS=\"--timeout 600 --access-logfile '-' --error-logfile '-'" +
		" --bind=0.0.0.0:12345 --chdir=/a/b/c\" gunicorn module.py"
	gen := PythonStartupScriptGenerator{
		BindPort: "12345",
	}

	// Act
	actual := gen.buildGunicornCommandForModule("module.py", "/a/b/c")

	// Assert
	assert.Equal(t, expected, actual)
}
