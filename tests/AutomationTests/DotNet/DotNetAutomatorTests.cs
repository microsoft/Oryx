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

namespace Microsoft.Oryx.Automation.DotNet.Tests
{
    public class DotNetAutomatorTests
    {
        private readonly IHttpServiceExtension httpService;
        private readonly IVersionService versionService;
        private readonly IFileService fileService;
        private readonly IYamlFileService yamlFileService;

        public DotNetAutomatorTests()
        {
            this.httpService = Mock.Of<IHttpServiceExtension>();
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
            var actual = await dotNetAutomator.GetNewDotNetVersionsAsync();

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}