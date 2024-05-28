// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package main

import (
	"fmt"
)

func Example_isLegacyDebuggerNeededifNoVersionAvailableDontUseLegacyDebugger() {
	// Act
	useLegacyDebugger := isLegacyDebuggerNeeded("")
	fmt.Print(useLegacyDebugger)
	// Output:
	// false
}

func Example_isLegacyDebuggerNeededNode4NeedsLegacyDebugger() {
	// Act
	useLegacyDebugger := isLegacyDebuggerNeeded("4.8.0")
	fmt.Print(useLegacyDebugger)
	// Output:
	// true
}

func Example_isLegacyDebuggerNeededNode611NeedsLegacyDebugger() {
	// Act
	useLegacyDebugger := isLegacyDebuggerNeeded("6.11.0")
	fmt.Print(useLegacyDebugger)
	// Output:
	// true
}

func Example_isLegacyDebuggerNeededNode76NeedsLegacyDebugger() {
	// Act
	useLegacyDebugger := isLegacyDebuggerNeeded("7.6.6")
	fmt.Print(useLegacyDebugger)
	// Output:
	// true
}

func Example_isLegacyDebuggerNeededNode77NeedsNewDebugger() {
	// Act
	useLegacyDebugger := isLegacyDebuggerNeeded("7.7.6")
	fmt.Println(useLegacyDebugger)
	// Output:
	// false
}

func Example_isLegacyDebuggerNeededNode811NeedsNewDebugger() {
	// Act
	useLegacyDebugger := isLegacyDebuggerNeeded("8.11.0")
	fmt.Print(useLegacyDebugger)
	// Output:
	// false
}

func Example_isLegacyDebuggerNeededNode1011NeedsNewDebugger() {
	// Act
	useLegacyDebugger := isLegacyDebuggerNeeded("10.11.0")
	fmt.Print(useLegacyDebugger)
	// Output:
	// false
}
