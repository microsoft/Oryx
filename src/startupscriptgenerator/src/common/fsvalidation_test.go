// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"fmt"
	"io/ioutil"
	"os"
	"testing"

	"github.com/stretchr/testify/assert"
)

func ExampleGetSubPath_simpleSubPath() {
	result := GetSubPath("a/b", "a/b/c")
	fmt.Println(result)
	// Output:
	// c
}

func ExampleGetSubPath_equalPath() {
	result := GetSubPath("a/b", "a/b/")
	fmt.Println(result)
	// Output:
	//
}

func ExampleGetSubPath_twoLevels() {
	result := GetSubPath("a/b/", "a/b/cde/fghi.abc")
	fmt.Println(result)
	// Output:
	// cde/fghi.abc
}

// test pathexists with valid path
func Test_FSValidation_PathExists_PathIsValid(t *testing.T) {
	testPath := os.TempDir()
	result := PathExists(testPath)
	fmt.Println(result)
	assert.Equal(t, result, true)
}

// test pathexists with invalid path
func Test_FSValidation_PathExists_PathIsInvalid(t *testing.T) {
	testPath := "asdasda"
	result := PathExists(testPath)
	fmt.Println(result)
	assert.Equal(t, result, false)
}

// test fileexists when path is a directory
func Test_FSValidation_FileExists_PathIsDirectory(t *testing.T) {
	filePath := os.TempDir()
	result := FileExists(filePath)
	fmt.Println(result)
	assert.Equal(t, result, false)
}

// test fileexists when path is a file
func Test_FSValidation_FileExists_PathIsFile(t *testing.T) {
	tmpfile, _ := ioutil.TempFile(os.TempDir(), "example-")
	result := FileExists(tmpfile.Name())
	fmt.Println(result)
	assert.Equal(t, result, true)
}

// test fileexists when path is invalid
func Test_FSValidation_FileExists_PathIsInvalid(t *testing.T) {
	filePath := "asdasda"
	result := FileExists(filePath)
	fmt.Println(result)
	assert.Equal(t, result, false)
}

// test addpermission when input is a filepath
func Test_FSValidation_AddPermission_ToFile(t *testing.T) {
	tmpfile, _ := ioutil.TempFile(os.TempDir(), "example-")
	result := TryAddPermission(tmpfile.Name(), 0755)
	fmt.Println(result)
	assert.Equal(t, result, true)

	defer os.Remove(tmpfile.Name())
}

// test addpermission() when input is a simple string not a filepath

func Test_FSValidation_AddPermission_ToString(t *testing.T) {
	filePath := "asdasda"
	result := TryAddPermission(filePath, 0755)
	fmt.Println(result)
	assert.Equal(t, result, false)
}

// test when input command string has a filepath
func Test_FSValidation_ParseCommandAndAddExecutionPermission_WithFile(t *testing.T) {
	tmpfile, _ := ioutil.TempFile(os.TempDir(), "example-")
	result := ParseCommandAndAddExecutionPermission(tmpfile.Name(), os.TempDir())
	fmt.Println(result)
	assert.Equal(t, result, true)

	defer os.Remove(tmpfile.Name())
}

// test when input command string doesn't have a file path
func Test_FSValidation_ParseCommandAndAddExecutionPermission_WithOutFile(t *testing.T) {
	command := "node lkajs asdkj nsad && "

	result := ParseCommandAndAddExecutionPermission(command, "asdasd")
	fmt.Println("result is ", command)

	assert.Equal(t, result, false)
}
