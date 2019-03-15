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

//test pathexists with valid path
func ExampleFSValidation_PathExists_PathIsValid(t *testing.T) {
	testPath := os.TempDir()
	result := FileExists(testPath)
	fmt.Println(result)
	assert.Equal(t, result, true)
}

//test pathexists with invalid path
func ExampleFSValidation_PathExists_PathIsInvalid(t *testing.T) {
	testPath := "asdasda"
	result := FileExists(testPath)
	fmt.Println(result)
	assert.Equal(t, result, false)
}

//test fileexists when path is a directory
func ExampleFSValidation_FileExists_PathIsDirectory(t *testing.T) {
	filePath := os.TempDir()
	result := FileExists(filePath)
	fmt.Println(result)
	assert.Equal(t, result, false)
}

//test fileexists when path is a file
func ExampleFSValidation_FileExists_PathIsFile(t *testing.T) {
	tmpfile, _ := ioutil.TempFile(os.TempDir(), "example-")
	result := FileExists(tmpfile.Name())
	fmt.Println(result)
	assert.Equal(t, result, true)
}

//test fileexists when path is invalid
func ExampleFSValidation_FileExists_PathIsInvalid(t *testing.T) {
	filePath := "asdasda"
	result := FileExists(filePath)
	fmt.Println(result)
	assert.Equal(t, result, false)
}

// test addpermission when input is a filepath
func ExampleFSValidation_AddPermission_ToFile(t *testing.T) {
	tmpfile, _ := ioutil.TempFile(os.TempDir(), "example-")

	info1, _ := os.Stat(tmpfile.Name())
	before := info1.Mode().String()
	fmt.Println("Before changing Permission ...", before)
	assert.NotContains(t, before, "x")

	result := AddPermission(tmpfile.Name(), 0755)
	fmt.Println(result)
	assert.Equal(t, result, true)

	info2, _ := os.Stat(tmpfile.Name())
	after := info2.Mode().String()
	fmt.Println("After changing Permission ...", after)

	defer os.Remove(tmpfile.Name())

	assert.NotEqual(t, before, after)
	assert.Contains(t, after, "x")
}

// test addpermission() when input is a simple string not a filepath

func ExampleFSValidation_AddPermission_ToString(t *testing.T) {
	filePath := "asdasda"
	permission := 0755
	result := AddPermission(filePath, permission)
	fmt.Println(result)
	assert.Equal(t, result, false)
}

// test when input command string has a filepath
func ExampleFSValidation_ParseCommandAndAddExecutionPermission_WithFile(t *testing.T) {
	tmpfile, _ := ioutil.TempFile(os.TempDir(), "example-")
	command := "node lkajs asdkj nsad && " + tmpfile.Name()

	info1, _ := os.Stat(tmpfile.Name())
	before := info1.Mode().String()
	assert.NotContains(t, before, "x")

	result := ParseCommandAndAddExecutionPermission(command, os.TempDir())
	fmt.Println(result)
	assert.Equal(t, result, true)

	info2, _ := os.Stat(tmpfile.Name())
	after := info2.Mode().String()

	defer os.Remove(tmpfile.Name())

	assert.NotEqual(t, before, after)
	assert.Contains(t, after, "x")
}

// test when input command string doesn't have a file path
func ExampleFSValidation_ParseCommandAndAddExecutionPermission_WithOutFile(t *testing.T) {
	command := "node lkajs asdkj nsad && "

	result := ParseCommandAndAddExecutionPermission(command, "asdasd")
	fmt.Println("result is ", command)

	assert.Equal(t, result, false)
}