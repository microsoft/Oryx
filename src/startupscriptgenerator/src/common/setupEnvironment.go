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

	// Check if ACR SDK provider is enabled (external or direct)
	enableExternalAcrSdkProvider := os.Getenv(consts.EnableExternalAcrSdkProviderKey)
	enableAcrSdkProvider := os.Getenv(consts.EnableAcrSdkProviderKey)
	if strings.EqualFold(enableExternalAcrSdkProvider, "true") || enableExternalAcrSdkProvider == "1" ||
		strings.EqualFold(enableAcrSdkProvider, "true") || enableAcrSdkProvider == "1" {
		return GetAcrSetupScript(platformName, version, installationDir, sentinelFilePath)
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
			fmt.Sprintf("curl --fail -D headers.txt -SL \"%s/%s/%s-%s.tar.gz%s\" --output %s\n",
				sdkStorageBaseUrl,
				platformName,
				platformName,
				version,
				sasToken,
				tarFile))
	} else {
		scriptBuilder.WriteString(
			fmt.Sprintf("curl --fail -D headers.txt -SL \"%s/%s/%s-%s-%s.tar.gz%s\" --output %s\n",
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

// GetAcrSetupScript generates a bash script to download an SDK from ACR using the OCI Distribution API.
// The script uses curl to fetch the manifest, extract the layer digest, download the blob,
// verify its SHA256 checksum, and extract the tarball.
func GetAcrSetupScript(platformName string, version string, installationDir string, sentinelFilePath string) string {
	acrRegistryUrl := os.Getenv(consts.AcrSdkRegistryUrlKeyName)
	if acrRegistryUrl == "" {
		acrRegistryUrl = consts.DefaultAcrSdkRegistryUrl
	}

	// Enforce HTTPS
	if !strings.HasPrefix(acrRegistryUrl, "https://") {
		panic(fmt.Sprintf("ACR registry URL must use HTTPS: '%s'", acrRegistryUrl))
	}

	// Remove trailing slash
	acrRegistryUrl = strings.TrimRight(acrRegistryUrl, "/")

	debianFlavor := os.Getenv(consts.DebianFlavor)
	if debianFlavor == "" {
		debianFlavor = "bookworm"
	}

	repository := fmt.Sprintf("%s/%s", consts.AcrSdkRepositoryPrefix, platformName)
	tag := fmt.Sprintf("%s-%s", debianFlavor, version)

	scriptBuilder := strings.Builder{}
	scriptBuilder.WriteString("#!/bin/sh\n")
	scriptBuilder.WriteString("set -e\n")
	scriptBuilder.WriteString("echo\n")
	scriptBuilder.WriteString(
		fmt.Sprintf("echo Downloading '%s' version '%s' from ACR to '%s'...\n", platformName, version, installationDir))
	scriptBuilder.WriteString(fmt.Sprintf("mkdir -p %s\n", installationDir))
	scriptBuilder.WriteString(fmt.Sprintf("cd %s\n", installationDir))

	// Fetch the OCI manifest (--fail ensures HTTP errors are not silently treated as content)
	manifestUrl := fmt.Sprintf("%s/v2/%s/manifests/%s", acrRegistryUrl, repository, tag)
	scriptBuilder.WriteString(fmt.Sprintf(
		"echo Fetching OCI manifest from ACR for %s:%s...\n", repository, tag))
	scriptBuilder.WriteString(fmt.Sprintf(
		"MANIFEST=$(curl --fail -sSL -H 'Accept: application/vnd.oci.image.manifest.v1+json' '%s')\n", manifestUrl))

	// Extract the layer digest (first/only layer in a FROM scratch image)
	scriptBuilder.WriteString(
		"LAYER_DIGEST=$(echo \"$MANIFEST\" | grep -o '\"digest\":\"sha256:[a-f0-9]*\"' | tail -1 | cut -d'\"' -f4)\n")
	scriptBuilder.WriteString("if [ -z \"$LAYER_DIGEST\" ]; then\n")
	scriptBuilder.WriteString("  echo 'ERROR: Could not extract layer digest from ACR manifest.'\n")
	scriptBuilder.WriteString("  exit 1\n")
	scriptBuilder.WriteString("fi\n")
	scriptBuilder.WriteString("echo \"Layer digest: $LAYER_DIGEST\"\n")

	// Extract expected SHA256 from digest
	scriptBuilder.WriteString("EXPECTED_SHA256=$(echo \"$LAYER_DIGEST\" | cut -d':' -f2)\n")

	// Download the layer blob
	blobUrl := fmt.Sprintf("%s/v2/%s/blobs/", acrRegistryUrl, repository)
	scriptBuilder.WriteString("echo Downloading SDK blob from ACR...\n")
	scriptBuilder.WriteString(fmt.Sprintf(
		"curl --fail -sSL '%s'\"$LAYER_DIGEST\" --output sdk.tar.gz\n", blobUrl))

	// Verify SHA256 checksum
	scriptBuilder.WriteString("echo Verifying SHA256 checksum...\n")
	scriptBuilder.WriteString("ACTUAL_SHA256=$(sha256sum sdk.tar.gz | cut -d' ' -f1)\n")
	scriptBuilder.WriteString("if [ \"$ACTUAL_SHA256\" != \"$EXPECTED_SHA256\" ]; then\n")
	scriptBuilder.WriteString("  echo \"ERROR: SHA256 checksum mismatch. Expected: $EXPECTED_SHA256, Got: $ACTUAL_SHA256\"\n")
	scriptBuilder.WriteString("  rm -f sdk.tar.gz\n")
	scriptBuilder.WriteString("  exit 1\n")
	scriptBuilder.WriteString("fi\n")
	scriptBuilder.WriteString("echo Checksum verified.\n")

	// Extract and clean up
	scriptBuilder.WriteString("echo Extracting contents...\n")
	scriptBuilder.WriteString("tar -xzf sdk.tar.gz -C .\n")
	scriptBuilder.WriteString("rm -f sdk.tar.gz\n")
	scriptBuilder.WriteString(fmt.Sprintf("echo Done. Installed at '%s' (from ACR)\n", installationDir))
	scriptBuilder.WriteString(fmt.Sprintf("echo > %s\n", sentinelFilePath))
	scriptBuilder.WriteString("echo\n")
	return scriptBuilder.String()
}
