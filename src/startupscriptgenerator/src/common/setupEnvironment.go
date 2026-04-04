// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

package common

import (
	"bytes"
	"common/consts"
	"fmt"
	"log"
	"os"
	"os/exec"
	"path"
	"strings"
)

func SetupEnv(script string) {
	WriteScript(consts.SetupScriptLocation, script)
	cmd := exec.Command("/bin/sh", consts.SetupScriptLocation)
	var outBytes, errBytes bytes.Buffer
	cmd.Stdout = &outBytes
	cmd.Stderr = &errBytes
	err := cmd.Run()
	fmt.Println(outBytes.String(), errBytes.String())
	if err != nil {
		log.Fatal(err)
	}
}

func GetSetupScript(platformName string, version string, installationDir string) string {
	dowloadSentinelFileName := ".oryx-sentinel"
	sentinelFilePath := path.Join(installationDir, dowloadSentinelFileName)

	// Check if the version was already installed
	if PathExists(sentinelFilePath) {
		return ""
	}

	sdkStorageBaseUrl := os.Getenv(consts.SdkStorageBaseUrlKeyName)
	if sdkStorageBaseUrl == "" {
		panic("Environment variable " + consts.SdkStorageBaseUrlKeyName + " is required.")
	}

	sasToken := ""

	tarFile := fmt.Sprintf("%s.tar.gz", version)
	debianFlavor := os.Getenv(consts.DebianFlavor)
	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("set -e\n")
	scriptBuilder.WriteString("echo\n")
	scriptBuilder.WriteString(
		fmt.Sprintf("echo Downloading '%s' version '%s' to '%s'...\n", platformName, version, installationDir))
	scriptBuilder.WriteString(fmt.Sprintf("mkdir -p %s\n", installationDir))
	scriptBuilder.WriteString(fmt.Sprintf("cd %s\n", installationDir))
	if debianFlavor == "" || debianFlavor == consts.DebianStretch {
		scriptBuilder.WriteString(
			fmt.Sprintf("curl -D headers.txt -SL \"%s/%s/%s-%s.tar.gz%s\" --output %s\n",
				sdkStorageBaseUrl,
				platformName,
				platformName,
				version,
				sasToken,
				tarFile))
	} else {
		scriptBuilder.WriteString(
			fmt.Sprintf("curl -D headers.txt -SL \"%s/%s/%s-%s-%s.tar.gz%s\" --output %s\n",
				sdkStorageBaseUrl,
				platformName,
				platformName,
				debianFlavor,
				version,
				sasToken,
				tarFile))
	}
	// Search for header ignoring case
	scriptBuilder.WriteString("headerName=\"x-ms-meta-checksum\"\n")
	scriptBuilder.WriteString(fmt.Sprintf(
		"checksumHeader=$(cat headers.txt | grep -i $headerName: | tr -d '%s')\n",
		"\\r"))
	// Change header and value to lowercase
	scriptBuilder.WriteString("echo Verifying checksum...\n")
	scriptBuilder.WriteString("checksumHeader=$(echo $checksumHeader | tr '[A-Z]' '[a-z]')\n")
	scriptBuilder.WriteString("checksumValue=${checksumHeader#\"$headerName: \"}\n")
	scriptBuilder.WriteString("rm -f headers.txt\n")
	scriptBuilder.WriteString(fmt.Sprintf("echo \"$checksumValue %s.tar.gz\" | sha512sum -c -\n", version))
	scriptBuilder.WriteString("echo Extracting contents...\n")
	scriptBuilder.WriteString(fmt.Sprintf("tar -xzf %s -C .\n", tarFile))
	scriptBuilder.WriteString(fmt.Sprintf("rm -f %s\n", tarFile))
	scriptBuilder.WriteString(fmt.Sprintf("echo Done. Installed at '%s'\n", installationDir))
	scriptBuilder.WriteString(fmt.Sprintf("rm -f %s\n", tarFile))
	scriptBuilder.WriteString(fmt.Sprintf("echo > %s\n", sentinelFilePath))
	scriptBuilder.WriteString("echo\n")
	return scriptBuilder.String()
}
