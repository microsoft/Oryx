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
	"strings"
)

func SetupEnv(script string) {
	WriteScript(consts.SetupScriptLocation, script)
	cmd := exec.Command("/bin/sh", consts.SetupScriptLocation)
	var outb, errb bytes.Buffer
	cmd.Stdout = &outb
	cmd.Stderr = &errb
	err := cmd.Run()
	if err != nil {
		log.Fatal(err)
	}
	fmt.Println(outb.String(), errb.String())
}

func GetSetupScript(platformName string, version string, installationDir string) string {
	sdkStorageBaseUrl := os.Getenv(consts.SdkStorageBaseUrlKeyName)
	if sdkStorageBaseUrl == "" {
		panic("Environment variable " + consts.SdkStorageBaseUrlKeyName + " is required.")
	}

	tarFile := fmt.Sprintf("%s.tar.gz", version)
	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("set -e\n")
	scriptBuilder.WriteString("echo\n")
	scriptBuilder.WriteString(
		fmt.Sprintf("echo Downloading and installing '%s' version '%s'...\n", platformName, version))
	scriptBuilder.WriteString(fmt.Sprintf("rm -rf %s\n", installationDir))
	scriptBuilder.WriteString(fmt.Sprintf("mkdir -p %s\n", installationDir))
	scriptBuilder.WriteString(fmt.Sprintf("cd %s\n", installationDir))
	scriptBuilder.WriteString(
		fmt.Sprintf("curl -D headers.txt -SL \"%s/%s/%s-%s.tar.gz\" --output %s >/dev/null 2>&1\n",
			sdkStorageBaseUrl,
			platformName,
			platformName,
			version,
			tarFile))
	scriptBuilder.WriteString("headerName=\"x-ms-meta-checksum\"\n")
	scriptBuilder.WriteString(fmt.Sprintf(
		"checksumHeader=$(cat headers.txt | grep -i $headerName: | tr -d '%s')\n",
		"\\r"))
	scriptBuilder.WriteString("checksumHeader=${checksumHeader,,}\n")
	scriptBuilder.WriteString("rm -f headers.txt\n")
	scriptBuilder.WriteString("checksumValue=${checksumHeader#\"$headerName: \"}\n")
	scriptBuilder.WriteString(fmt.Sprintf("echo \"$checksumValue %s.tar.gz\" | sha512sum -c - >/dev/null 2>&1\n", version))
	scriptBuilder.WriteString(fmt.Sprintf("tar -xzf %s -C .\n", tarFile))
	scriptBuilder.WriteString(fmt.Sprintf("rm -f %s\n", tarFile))
	scriptBuilder.WriteString(fmt.Sprintf("echo Done. Installed at '%s'\n", installationDir))
	scriptBuilder.WriteString(fmt.Sprintf("rm -f %s\n", tarFile))
	scriptBuilder.WriteString("echo\n")
	return scriptBuilder.String()
}
