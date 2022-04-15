// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"fmt"
)

func ExampleHugoStartupScriptGenerator_GenerateEntrypointScript() {
	// Arrange
	gen := HugoStartupScriptGenerator{AppPath: "abc", UserStartupCommand: ""}
	// Act
	command := gen.GenerateEntrypointScript()
	fmt.Println(command)
}