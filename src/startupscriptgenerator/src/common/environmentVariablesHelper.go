// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"fmt"
	"os"
	"strconv"
)

func GetBooleanEnvironmentVariable(key string) bool {
	value := os.Getenv(key)
	if value == "" {
		return false
	}

	result, err := strconv.ParseBool(value)
	if err != nil {
		panic(fmt.Sprintf("Invalid value '%s' for '%s'. Value can either be 'true' or 'false'.", value, key))
	}
	return result
}
