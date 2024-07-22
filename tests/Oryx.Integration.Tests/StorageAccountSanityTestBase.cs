// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Integration.Tests;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace Oryx.Integration.Tests
{
    public abstract class StorageAccountSanityTestBase
        : PlatformEndToEndTestsBase, IClassFixture<RepoRootDirTestFixture>
    {
        private const string _fakeStorageUrl = "https://oryx-cdn-fake.microsoft.io";

        private readonly string _storageUrl;
        private readonly string _repoRootDir;

        private readonly string[] _debianFlavors = 
        {
            OsTypes.DebianBuster, OsTypes.DebianStretch, OsTypes.UbuntuFocalScm, OsTypes.DebianBullseye, OsTypes.DebianBookworm
        };

        public StorageAccountSanityTestBase(
            string storageUrl,
            ITestOutputHelper output,
            TestTempDirTestFixture testTempDirTestFixture,
            RepoRootDirTestFixture repoRootDirTestFixture)
            : base(output, testTempDirTestFixture)
        {
            _storageUrl = storageUrl;
            _repoRootDir = repoRootDirTestFixture.RepoRootDirPath;
        }

        [Fact]
        public void DotNetCoreContainer_HasExpectedListOfBlobs()
        {
            var platformName = "dotnet";
            AssertExpectedListOfBlobs(platformName, platformName);
        }

        [Fact]
        public void DotNetCoreContainer_HasExpectedDefaultVersion()
        {
            var platformName = "dotnet";
            AssertExpectedDefaultVersion(platformName, platformName);
        }

        [Fact]
        public void GolangCoreContainer_HasExpectedListOfBlobs()
        {
            var platformName = "golang";
            AssertExpectedListOfBlobs(platformName, platformName);
        }

        [Fact]
        public void GolangContainer_HasExpectedDefaultVersion()
        {
            var platformName = "golang";
            AssertExpectedDefaultVersion(platformName, platformName);
        }

        [Fact]
        public void PythonContainer_HasExpectedListOfBlobs()
        {
            var platformName = "python";
            AssertExpectedListOfBlobs(platformName, platformName);
        }

        [Fact]
        public void PythonContainer_HasExpectedDefaultVersion()
        {
            var platformName = "python";
            AssertExpectedDefaultVersion(platformName, platformName);
        }

        [Fact]
        public void NodeJSContainer_HasExpectedListOfBlobs()
        {
            // Arrange & Act
            var platformName = "nodejs";
            AssertExpectedListOfBlobs(platformName, platformName);
        }

        [Fact]
        public void NodeJSContainer_HasExpectedDefaultVersion()
        {
            var platformName = "nodejs";
            AssertExpectedDefaultVersion(platformName, platformName);
        }

        [Fact]
        public void PhpComposerCoreContainer_HasExpectedListOfBlobs()
        {
            var platformName = "php-composer";
            AssertExpectedListOfBlobs(platformName, "php", "composer");
        }

        [Fact]
        public void PhpContainer_HasExpectedListOfBlobs()
        {
            var platformName = "php";
            AssertExpectedListOfBlobs(platformName, platformName);
        }

        [Fact]
        public void PhpContainer_HasExpectedDefaultVersion()
        {
            var platformName = "php";
            AssertExpectedDefaultVersion(platformName, "php");
        }

        [Fact]
        public void PhpComposerContainer_HasExpectedDefaultVersion()
        {
            var platformName = "php-composer";
            AssertExpectedDefaultVersion(platformName, "php", "composer");
        }

        [Fact]
        public void RubyContainer_HasExpectedListOfBlobs()
        {
            var platformName = "ruby";
            AssertExpectedListOfBlobs(platformName, platformName);
        }

        [Fact]
        public void RubyContainer_HasExpectedDefaultVersion()
        {
            var platformName = "ruby";
            AssertExpectedDefaultVersion(platformName, platformName);
        }

        [Fact]
        public void JavaContainer_HasExpectedListOfBlobs()
        {
            var platformName = "java";
            AssertExpectedListOfBlobs(platformName, platformName);
        }

        [Fact]
        public void JavaContainer_HasExpectedDefaultVersion()
        {
            var platformName = "java";
            AssertExpectedDefaultVersion(platformName, platformName);

        }

        [Fact]
        public void MavenContainer_HasExpectedListOfBlobs()
        {
            AssertExpectedListOfBlobs("maven", "java", "maven");
        }

        [Fact]
        public void MavenContainer_HasExpectedDefaultVersion()
        {
            AssertExpectedDefaultVersion("maven", "java", "maven");
        }

        [Fact]
        public void Throws_CorrectHttpErrorMessage()
        {
            // Act
            var error = Assert.Throws<AggregateException>(() => 
                ListBlobsHelper.GetAllBlobs(_fakeStorageUrl, "dotnet", _httpClient));

            // Assert
            Assert.Contains(Microsoft.Oryx.BuildScriptGenerator.Constants.NetworkConfigurationHelpText, error.Message);
        }

        private void AssertExpectedDefaultVersion(string platformName, params string[] expectedPlatformPath)
        {
            foreach (var debianFlavor in _debianFlavors)
            {
                // Arrange & Act
                var actualVersion = GetDefaultVersionFromContainer(debianFlavor, platformName);
                var expectedVersion = GetDefaultVersion(debianFlavor, expectedPlatformPath);

                // Assert
                Assert.Equal(expectedVersion, actualVersion);
            }
        }

        private void AssertExpectedListOfBlobs(string platformName, params string[] expectedPlatformPath)
        {
            foreach (var debianFlavor in _debianFlavors)
            {
                // Arrange & Act
                var actualVersions = GetVersionsFromContainer(debianFlavor, platformName);
                var expectedVersions = GetListOfVersionsToBuild(debianFlavor, expectedPlatformPath);

                // Assert
                foreach (var expectedVersion in expectedVersions)
                {
                    Assert.Contains(expectedVersion, actualVersions);
                }
            }
        }

        private XDocument GetMetadata(string platformName)
        {
            return ListBlobsHelper.GetAllBlobs(_storageUrl, platformName, _httpClient);
        }

        private List<string> GetVersionsFromContainer(string debianFlavor, string platformName)
        {
            var xdoc = GetMetadata(platformName);
            var supportedVersions = new List<string>();
            var isStretch = string.Equals(debianFlavor, OsTypes.DebianStretch, StringComparison.OrdinalIgnoreCase);

            var sdkVersionMetadataName = isStretch
                ? SdkStorageConstants.LegacySdkVersionMetadataName
                : SdkStorageConstants.SdkVersionMetadataName;

            foreach (var metadataElement in xdoc.XPathSelectElements($"//Blobs/Blob/Metadata"))
            {
                var childElements = metadataElement.Elements();
                var versionElement = childElements
                    .Where(e => string.Equals(sdkVersionMetadataName, e.Name.LocalName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                var osTypeElement = childElements
                    .Where(e => string.Equals(SdkStorageConstants.OsTypeMetadataName, e.Name.LocalName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                // if a matching version element is not found, we do not add as a supported version
                // if the os type is stretch and we find a blob with a 'Version' metadata, we know it is a supported version
                // otherwise, we check the blob for 'Sdk_version' metadata AND ensure 'Os_type' metadata matches current debianFlavor
                if (versionElement != null &&
                    (isStretch || (osTypeElement != null && string.Equals(debianFlavor, osTypeElement.Value, StringComparison.OrdinalIgnoreCase))))
                {
                    supportedVersions.Add(versionElement.Value);
                }
            }

            return supportedVersions;
        }

        private string GetDefaultVersionFromContainer(string debianFlavor, string platformName)
        {
            var defaultFile = string.IsNullOrEmpty(debianFlavor)
                    || string.Equals(debianFlavor, OsTypes.DebianStretch, StringComparison.OrdinalIgnoreCase)
                ? SdkStorageConstants.DefaultVersionFileName
                : $"{SdkStorageConstants.DefaultVersionFilePrefix}.{debianFlavor}.{SdkStorageConstants.DefaultVersionFileType}";
            var defaultVersionUrl = $"{_storageUrl}/{platformName}/{defaultFile}";
            var defaultVersionContent = _httpClient.GetStringAsync(defaultVersionUrl).Result;

            string defaultVersion = null;
            using (var stringReader = new StringReader(defaultVersionContent))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
                {
                    // Ignore any comments in the file
                    if (!line.StartsWith("#") || !line.StartsWith("//"))
                    {
                        defaultVersion = line.Trim();
                        break;
                    }
                }
            }
            return defaultVersion;
        }

        private List<string> GetListOfVersionsToBuild(string debianFlavor, params string[] platformPath)
        {
            var platformSubPath = Path.Combine(platformPath);
            var versionFile = Path.Combine(
                _repoRootDir,
                "platforms",
                platformSubPath,
                "versions",
                debianFlavor,
                SdkStorageConstants.VersionsToBuildFileName);
            if (!File.Exists(versionFile))
            {
                throw new InvalidOperationException($"Could not find file '{versionFile}'");
            }

            var versions = new List<string>();
            using (var streamReader = new StreamReader(versionFile))
            {
                string line = null;
                while ((line = streamReader.ReadLine()) != null)
                {
                    // Remove extraneous whitespace
                    line = line.Trim();

                    // ignore comments or empty lines
                    if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                    {
                        continue;
                    }
                    var parts = line.Split(",");
                    versions.Add(parts[0].Trim());
                }
            }

            return versions;
        }

        private string GetDefaultVersion(string debianFlavor, params string[] platformPath)
        {
            var platformSubPath = Path.Combine(platformPath);
            var file = Path.Combine(
                _repoRootDir,
                "platforms",
                platformSubPath,
                "versions",
                debianFlavor,
                SdkStorageConstants.DefaultVersionFileName);
            if (!File.Exists(file))
            {
                throw new InvalidOperationException($"Could not file default version file '{file}'.");
            }

            string defaultVersion = null;
            using (var streamReader = new StreamReader(file))
            {
                string line = null;
                while ((line = streamReader.ReadLine()) != null)
                {
                    // Remove extraneous whitespace
                    line = line.Trim();

                    // ignore comments or empty lines
                    if (line.StartsWith("#") || string.IsNullOrEmpty(line))
                    {
                        continue;
                    }

                    defaultVersion = line.Trim();
                }
            }

            if (string.IsNullOrEmpty(defaultVersion))
            {
                throw new InvalidOperationException("Default version cannot be empty");
            }

            return defaultVersion;
        }
    }
}
