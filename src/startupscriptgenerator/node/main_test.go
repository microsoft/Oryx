package main

import (
	"fmt"
	"os"
)

func Example_isLegacyDebuggerNeededifNoVersionAvailableDontUseLegacyDebugger() {
	useLegacyDebugger := isLegacyDebuggerNeeded()
	fmt.Print(useLegacyDebugger)
	// Output:
	// false
}

func Example_isLegacyDebuggerNeededNode4NeedsLegacyDebugger() {
	// Arrange
	os.Setenv("NODE_VERSION", "4.8.0")
	useLegacyDebugger := isLegacyDebuggerNeeded()
	fmt.Print(useLegacyDebugger)
	// Output:
	// true
}

func Example_isLegacyDebuggerNeededNode611NeedsLegacyDebugger() {
	// Arrange
	os.Setenv("NODE_VERSION", "6.11.0")
	useLegacyDebugger := isLegacyDebuggerNeeded()
	fmt.Print(useLegacyDebugger)
	// Output:
	// true
}

func Example_isLegacyDebuggerNeededNode76NeedsLegacyDebugger() {
	// Arrange
	os.Setenv("NODE_VERSION", "7.6.6")
	useLegacyDebugger := isLegacyDebuggerNeeded()
	fmt.Print(useLegacyDebugger)
	// Output:
	// true
}

func Example_isLegacyDebuggerNeededNode77NeedsNewDebugger() {
	// Arrange
	os.Setenv("NODE_VERSION", "7.7.6")
	useLegacyDebugger := isLegacyDebuggerNeeded()
	fmt.Println(useLegacyDebugger)
	// Output:
	// false
}

func Example_isLegacyDebuggerNeededNode811NeedsNewDebugger() {
	// Arrange
	os.Setenv("NODE_VERSION", "8.11.0")
	useLegacyDebugger := isLegacyDebuggerNeeded()
	fmt.Print(useLegacyDebugger)
	// Output:
	// false
}

func Example_isLegacyDebuggerNeededNode1011NeedsNewDebugger() {
	// Arrange
	os.Setenv("NODE_VERSION", "10.11.0")
	useLegacyDebugger := isLegacyDebuggerNeeded()
	fmt.Print(useLegacyDebugger)
	// Output:
	// false
}
