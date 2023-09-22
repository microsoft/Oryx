// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Oryx.Automation.DotNet.Models;
using Microsoft.Oryx.Automation.Services;
using System;

namespace Microsoft.Oryx.Automation.DotNet.Tests
{
    public class DotNetAutomatorTests
    {
        private readonly IHttpService httpService;
        private readonly IVersionService versionService;
        private readonly IFileService fileService;
        private readonly IYamlFileService yamlFileService;

        public DotNetAutomatorTests()
        {
            this.httpService = Mock.Of<IHttpService>();
            this.versionService = Mock.Of<IVersionService>();
            this.fileService = Mock.Of<IFileService>();
            this.yamlFileService = Mock.Of<IYamlFileService>();
        }

        [Fact]
        public async Task GetNewDotNetVersionsAsync_ReturnsEmptyList_WhenNoNewVersions()
        {
            // Arrange
            var dotNetAutomator = new DotNetAutomator(this.httpService, this.versionService, this.fileService, this.yamlFileService);
            var expected = new List<DotNetVersion>();

            Mock.Get(this.httpService).Setup(x => x.GetDataAsync(It.IsAny<string>())).ReturnsAsync("");

            // Act
            await dotNetAutomator.InitializeFieldsAsync();
            var actual = await dotNetAutomator.GetNewDotNetVersionsAsync();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task ShouldAcceptDotnetEightPreRelease()
        {
            // Arrange
            var versionService = new VersionService();
            var dotNetAutomator = new DotNetAutomator(this.httpService, versionService, this.fileService, this.yamlFileService);
            var expected = new List<DotNetVersion>()
            {
                new DotNetVersion()
                {
                    Sha = "test-sha1",
                    Version = "8.0.0-rc.1.23419.4",
                    VersionType = "net-core"
                },
                new DotNetVersion()
                {
                    Sha = "test-sha3",
                    Version = "8.0.0-rc.1.23421.29",
                    VersionType = "aspnet-core"
                },
                new DotNetVersion()
                {
                    Sha = "test-sha2",
                    Version = "8.0.100-rc.1.23463.5",
                    VersionType = "sdk"

                }
            };

            string DotnetEightPreReleaseIndexJsonReturnMock = @"{
    ""$schema"": ""https://json.schemastore.org/dotnet-releases-index.json"",
    ""releases-index"": [
        {
            ""channel-version"": ""8.0"",
            ""latest-release"": ""8.0.0-rc.1"",
            ""latest-release-date"": ""2023-09-14"",
            ""security"": true,
            ""latest-runtime"": ""8.0.0-rc.1.23419.4"",
            ""latest-sdk"": ""8.0.100-rc.1.23463.5"",
            ""product"": "".NET"",
            ""release-type"": ""lts"",
            ""support-phase"": ""go-live"",
            ""eol-date"": null,
            ""releases.json"": ""https://mock-test-url/dotnet/release-metadata/8.0/releases.json""
        }]
}";
            Mock.Get(this.httpService).Setup(x => x.GetDataAsync(DotNetConstants.ReleasesIndexJsonUrl)).ReturnsAsync(DotnetEightPreReleaseIndexJsonReturnMock);

            Mock.Get(this.httpService).Setup(x => x.GetOryxSdkVersionsAsync(It.IsAny<string>())).ReturnsAsync(new HashSet<string>());

            string DotnetEightPreReleaseReleaseJsonMock = @"{
    ""channel-version"": ""8.0"",
    ""latest-release"": ""8.0.0-rc.1"",
    ""latest-release-date"": ""2023-09-14"",
    ""latest-runtime"": ""8.0.0-rc.1.23419.4"",
    ""latest-sdk"": ""8.0.100-rc.1.23463.5"",
    ""releases"": [
        {
            ""release-version"": ""8.0.0-rc.1"",
            ""runtime"": {
                ""version"": ""8.0.0-rc.1.23419.4"",
                ""version-display"": ""8.0.0-rc.1"",
                ""vs-version"": """",
                ""vs-mac-version"": """",
                ""files"": [
                    {
                        ""name"": ""dotnet-runtime-linux-x64.tar.gz"",
                        ""rid"": ""linux-x64"",
                        ""url"": ""https://automator-mock-test.com/download/pr/dotnet-runtime-8.0.0-rc.1.23419.4-linux-x64.tar.gz"",
                        ""hash"": ""test-sha1""
                    }
                ]
            },
            ""sdk"": {
                ""version"": ""8.0.100-rc.1.23463.5"",
                ""version-display"": ""8.0.100-rc.1"",
                ""runtime-version"": ""8.0.0-rc.1.23419.4"",
                ""files"": [
                    {
                        ""name"": ""dotnet-sdk-linux-x64.tar.gz"",
                        ""rid"": ""linux-x64"",
                        ""url"": ""https://automator-mock-test.com/download/pr/mock/dotnet-sdk-8.0.100-rc.1.23463.5-linux-x64.tar.gz"",
                        ""hash"": ""test-sha2""
                    }
                ]
            },
            ""aspnetcore-runtime"": {
                ""version"": ""8.0.0-rc.1.23421.29"",
                ""version-display"": ""8.0.0-rc.1"",
                ""version-aspnetcoremodule"": [
                    ""18.0.23234.0""
                ],
                ""vs-version"": """",
                ""files"": [
                    {
                        ""name"": ""aspnetcore-runtime-linux-x64.tar.gz"",
                        ""rid"": ""linux-x64"",
                        ""url"": ""https://automator-mock-test.com/download/pr/mock/aspnetcore-runtime-8.0.0-rc.1.23421.29-linux-x64.tar.gz"",
                        ""hash"": ""test-sha3""
                    }
                ]
            },
        }
    ]
}";
            Mock.Get(this.httpService).Setup(x => x.GetDataAsync("https://mock-test-url/dotnet/release-metadata/8.0/releases.json")).ReturnsAsync(DotnetEightPreReleaseReleaseJsonMock);
            // Act
            await dotNetAutomator.InitializeFieldsAsync();
            var actual = await dotNetAutomator.GetNewDotNetVersionsAsync();

            // Assert
            Assert.Equal(expected, actual);

        }
    }
}