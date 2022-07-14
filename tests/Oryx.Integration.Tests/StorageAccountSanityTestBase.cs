// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Integration.Tests;
using Microsoft.Oryx.Tests.Common;
using Xunit;
using Xunit.Abstractions;

namespace Oryx.Integration.Tests
{
    public abstract class StorageAccountSanityTestBase
        : PlatformEndToEndTestsBase, IClassFixture<RepoRootDirTestFixture>
    {
        private readonly string _storageUrl;
        private readonly string _repoRootDir;

        private readonly string[] _debianFlavors = { "buster", "bullseye", "vso-focal", "stretch" };

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
            // Arrange & Act
            var platformName = "dotnet";
            var actualVersions = GetVersionsFromContainer(platformName, "version");
            var expectedVersions = GetListOfVersionsToBuild(platformName);

            // Assert
            foreach (var expectedVersion in expectedVersions)
            {
                Assert.Contains(expectedVersion, actualVersions);
            }
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
            // Arrange & Act
            var platformName = "golang";
            var actualVersions = GetVersionsFromContainer(platformName, "version");
            var expectedVersions = GetListOfVersionsToBuild(platformName);

            // Assert
            foreach (var expectedVersion in expectedVersions)
            {
                Assert.Contains(expectedVersion, actualVersions);
            }
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
            // Arrange & Act
            var platformName = "python";
            var actualVersions = GetVersionsFromContainer(platformName, "version");
            var expectedVersions = GetListOfVersionsToBuild(platformName);

            // Assert
            foreach (var expectedVersion in expectedVersions)
            {
                Assert.Contains(expectedVersion, actualVersions);
            }
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
            var actualVersions = GetVersionsFromContainer(platformName, "version");
            var expectedVersions = GetListOfVersionsToBuild(platformName);

            // Assert
            foreach (var expectedVersion in expectedVersions)
            {
                Assert.Contains(expectedVersion, actualVersions);
            }
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
            // Arrange & Act
            var platformName = "php-composer";
            var actualVersions = GetVersionsFromContainer(platformName, "version");
            var expectedVersions = GetListOfVersionsToBuild("php", "composer");

            // Assert
            foreach (var expectedVersion in expectedVersions)
            {
                Assert.Contains(expectedVersion, actualVersions);
            }
        }

        [Fact]
        public void PhpContainer_HasExpectedListOfBlobs()
        {
            // Arrange & Act
            var platformName = "php";
            var actualVersions = GetVersionsFromContainer(platformName, "version");
            var expectedVersions = GetListOfVersionsToBuild(platformName);

            // Assert
            foreach (var expectedVersion in expectedVersions)
            {
                Assert.Contains(expectedVersion, actualVersions);
            }
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
            // Arrange & Act
            var platformName = "ruby";
            var actualVersions = GetVersionsFromContainer(platformName, "version");
            var expectedVersions = GetListOfVersionsToBuild(platformName);

            // Assert
            foreach (var expectedVersion in expectedVersions)
            {
                Assert.Contains(expectedVersion, actualVersions);
            }
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
            // Arrange & Act
            var platformName = "java";
            var actualVersions = GetVersionsFromContainer(platformName, "version");
            var expectedVersions = GetListOfVersionsToBuild(platformName);

            // Assert
            foreach (var expectedVersion in expectedVersions)
            {
                Assert.Contains(expectedVersion, actualVersions);
            }
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
            AssertExpectedListOfBlobs("maven", "version", "java", "maven");
        }

        [Fact]
        public void MavenContainer_HasExpectedDefaultVersion()
        {
            AssertExpectedDefaultVersion("maven", "java", "maven");
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

        private void AssertExpectedListOfBlobs(string platformName, string metadataElementName, params string[] expectedPlatformPath)
        {
            foreach (var debianFlavor in _debianFlavors)
            {
                // Arrange & Act
                var actualVersions = GetVersionsFromContainer(platformName, metadataElementName);
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
            var url = string.Format(SdkStorageConstants.ContainerMetadataUrlFormat, _storageUrl, platformName);
            var blobList = _httpClient.GetStringAsync(url).Result;
            return XDocument.Parse(blobList);
        }

        private List<string> GetVersionsFromContainer(string platformName, string metadataElementName)
        {
            var xdoc = GetMetadata(platformName);
            var supportedVersions = new List<string>();
            foreach (var metadataElement in xdoc.XPathSelectElements($"//Blobs/Blob/Metadata"))
            {
                var childElements = metadataElement.Elements();
                var versionElement = childElements.Where(e => string.Equals(
                        metadataElementName,
                        e.Name.LocalName,
                        StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();
                if (versionElement != null)
                {
                    supportedVersions.Add(versionElement.Value);
                }
            }
            return supportedVersions;
        }

        private string GetDefaultVersionFromContainer(string debianFlavor, string platformName)
        {
            // TODO: replace this with the defaultVersion.{debianFlavor}.txt once we actually have the blobs in the
            // storage account
            var defaultVersionContent = _httpClient
                .GetStringAsync($"{_storageUrl}/{platformName}/{SdkStorageConstants.DefaultVersionFileName}")
                .Result;

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
                while ((line = streamReader.ReadLine()) != null && !string.IsNullOrEmpty(line))
                {
                    // ignore comments
                    if (line.StartsWith("#"))
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
                while ((line = streamReader.ReadLine()) != null && !string.IsNullOrEmpty(line))
                {
                    // ignore comments
                    if (line.StartsWith("#"))
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
