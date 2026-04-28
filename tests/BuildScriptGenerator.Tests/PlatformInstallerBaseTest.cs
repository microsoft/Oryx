// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Node;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    /// <summary>
    /// Tests for <see cref="PlatformInstallerBase.GetInstallerScriptSnippet"/> exercised
    /// through <see cref="NodePlatformInstaller"/>.
    /// </summary>
    public class PlatformInstallerBaseTest
    {

        [Fact]
        public void GetInstallerScriptSnippet_WhenSkipDownload_ChecksExternalSdkDir()
        {
            var installer = CreateInstaller(debianFlavor: "bookworm");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: true);

            Assert.Contains("/var/OryxSdks", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenSkipDownload_ChecksExternalAcrSdkDir()
        {
            var installer = CreateInstaller(debianFlavor: "bookworm");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: true);

            Assert.Contains("/var/OryxAcrSdks", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenSkipDownload_ExitsOnMissingTarball()
        {
            var installer = CreateInstaller(debianFlavor: "bookworm");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: true);

            Assert.Contains("exit 1", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenSkipDownload_DoesNotContainSdkDownloadCurl()
        {
            var installer = CreateInstaller(debianFlavor: "bookworm");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: true);

            // Should not contain the SDK binary download curl
            Assert.DoesNotContain("curl --connect-timeout", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenSkipDownload_ChecksDynamicInstallDir()
        {
            var dynamicDir = "/opt/oryx-dynamic";
            var installer = CreateInstaller(debianFlavor: "bookworm", dynamicInstallRootDir: dynamicDir);

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: true);

            Assert.Contains(dynamicDir, snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenSkipDownload_UsesFlavorInTarballName()
        {
            var installer = CreateInstaller(debianFlavor: "bookworm");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: true);

            Assert.Contains("nodejs-bookworm-20.0.0.tar.gz", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenSkipDownload_Stretch_UsesLegacyTarballName()
        {
            var installer = CreateInstaller(debianFlavor: OsTypes.DebianStretch);

            var snippet = installer.GetInstallerScriptSnippet("14.0.0", skipSdkBinaryDownload: true);

            // Stretch uses the legacy format: {platform}-{version}.tar.gz (no flavor in tarball name)
            Assert.Contains("nodejs-14.0.0.tar.gz", snippet);
            Assert.DoesNotContain("nodejs-stretch-14.0.0.tar.gz", snippet);
        }


        [Fact]
        public void GetInstallerScriptSnippet_WhenNotSkipDownload_UsesCurl()
        {
            var installer = CreateInstaller(debianFlavor: "bookworm");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: false);

            Assert.Contains("curl", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenNotSkipDownload_ContainsPrimaryUrl()
        {
            var installer = CreateInstaller(debianFlavor: "bookworm", sdkStorageUrl: "https://primary.example.com");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: false);

            Assert.Contains("https://primary.example.com", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenNotSkipDownload_ContainsBackupUrl()
        {
            var installer = CreateInstaller(
                debianFlavor: "bookworm",
                sdkStorageUrl: "https://primary.example.com",
                sdkStorageBackupUrl: "https://backup.example.com");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: false);

            Assert.Contains("https://backup.example.com", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenNotSkipDownload_EmbedsDebianFlavorInUrl()
        {
            var installer = CreateInstaller(debianFlavor: "bookworm");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: false);

            // The DEBIAN_FLAVOR variable is exported, and the curl line references $DEBIAN_FLAVOR
            Assert.Contains("DEBIAN_FLAVOR=bookworm", snippet);
            Assert.Contains("nodejs-$DEBIAN_FLAVOR-20.0.0.tar.gz", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenNotSkipDownload_Stretch_UsesLegacyUrlPattern()
        {
            var installer = CreateInstaller(debianFlavor: OsTypes.DebianStretch);

            var snippet = installer.GetInstallerScriptSnippet("14.0.0", skipSdkBinaryDownload: false);

            // Stretch path: {platform}-{version}.tar.gz (no $DEBIAN_FLAVOR)
            Assert.Contains("nodejs-14.0.0.tar.gz", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WhenNotSkipDownload_DoesNotContainExitOne()
        {
            // The curl path handles failures via the backup URL, not exit 1 from tarball lookup
            var installer = CreateInstaller(debianFlavor: "bookworm");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: false);

            // Should not have the "Could not find cached tarball" exit 1
            Assert.DoesNotContain("Could not find cached tarball", snippet);
        }

        [Fact]
        public void GetInstallerScriptSnippet_WritesSentinelFile()
        {
            var installer = CreateInstaller(debianFlavor: "bookworm");

            var snippet = installer.GetInstallerScriptSnippet("20.0.0", skipSdkBinaryDownload: false);

            Assert.Contains(SdkStorageConstants.SdkDownloadSentinelFileName, snippet);
        }


        private static NodePlatformInstaller CreateInstaller(
            string debianFlavor,
            string dynamicInstallRootDir = "/opt/oryx",
            string sdkStorageUrl = "https://sdk.example.com",
            string sdkStorageBackupUrl = "https://sdk-backup.example.com")
        {
            var options = Options.Create(new BuildScriptGeneratorOptions
            {
                DebianFlavor = debianFlavor,
                DynamicInstallRootDir = dynamicInstallRootDir,
                OryxSdkStorageBaseUrl = sdkStorageUrl,
                OryxSdkStorageBackupBaseUrl = sdkStorageBackupUrl,
            });

            return new NodePlatformInstaller(options, NullLoggerFactory.Instance);
        }
    }
}
