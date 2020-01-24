// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"fmt"
)

func ExamplePhpStartupScriptGenerator_GenerateEntrypointScript() {
	// Arrange
	gen := PhpStartupScriptGenerator{SourcePath: "abc", StartupCmd: "somecmd"}
	// Act
	command := gen.GenerateEntrypointScript()
	fmt.Println(command)
	// Output:
	// #!/bin/sh
	// # Enter the source directory to make sure the script runs where the user expects
	// cd abc
	// if [ -z "$APACHE_PORT" ]; then
	// 		export APACHE_PORT=8080
	// fi
	//
	// if [  -n "$PHP_ORIGIN" ] && [ "$PHP_ORIGIN" = "php-fpm" ]; then
	//    export NGINX_DOCUMENT_ROOT='abc'
	//    service nginx start
	// else
	//    export APACHE_DOCUMENT_ROOT='abc'
	// fi
	//
	// somecmd
}
