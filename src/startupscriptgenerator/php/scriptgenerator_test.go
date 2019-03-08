// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"fmt"
)

func ExamplePhpStartupScriptGenerator_GenerateEntrypointScript_SourcePathIsExported() {
	// Arrange
	gen := PhpStartupScriptGenerator { SourcePath: "abc", StartupCmd: "", }
	// Act
	command := gen.GenerateEntrypointScript()
	fmt.Println(command)
	// Output:
	// export APACHE_DOCUMENT_ROOT='abc'
}

func ExamplePhpStartupScriptGenerator_GenerateEntrypointScript_StartupCommandIsUsed() {
	// Arrange
	gen := PhpStartupScriptGenerator { SourcePath: "", StartupCmd: "somecmd", }
	// Act
	command := gen.GenerateEntrypointScript()
	fmt.Println(command)
	// Output:
	// somecmd
}
