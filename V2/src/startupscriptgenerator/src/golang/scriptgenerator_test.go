// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"fmt"
)

func ExampleGolangStartupScriptGenerator_GenerateEntrypointScript() {
	// Arrange
	gen := GolangStartupScriptGenerator{AppPath: "abc", UserStartupCommand: ""}
	// Act
	command := gen.GenerateEntrypointScript()
	fmt.Println(command)
}