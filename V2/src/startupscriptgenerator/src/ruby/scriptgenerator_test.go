// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"fmt"
)

func ExampleRubyStartupScriptGenerator_GenerateEntrypointScript() {
	// Arrange
	gen := RubyStartupScriptGenerator{SourcePath: "abc", UserStartupCommand: ""}
	// Act
	command := gen.GenerateEntrypointScript()
	fmt.Println(command)
}