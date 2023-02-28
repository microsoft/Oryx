// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Oryx.Automation.Models;
using Microsoft.Oryx.Automation.Services;

namespace Microsoft.Oryx.Automation.Tests.Services
{
    public class YamlFileServiceTests
    {
        private readonly YamlFileService yamlFileService;
        private readonly string oryxRootPath;
        private readonly string testConstantsYamlFilePath;

        public YamlFileServiceTests()
        {
            this.oryxRootPath = Directory.GetCurrentDirectory();
            this.testConstantsYamlFilePath = Path.Combine(this.oryxRootPath, "test-constants.yaml");
            this.yamlFileService = new YamlFileService(this.oryxRootPath);

        }

        [Fact]
        public async Task ReadConstantsYamlFileAsync_ReturnsExpectedYamlContents()
        {
            // Arrange
            string yamlContents = "- name: test\r\n  constants:\r\n    key1: value1\r\n    key2: value2";
            File.WriteAllText(this.testConstantsYamlFilePath, yamlContents);

            // Act
            var result = await this.yamlFileService.ReadConstantsYamlFileAsync(this.testConstantsYamlFilePath);

            // Assert
            Assert.Single(result);
            Assert.Equal("value1", result[0].Constants["key1"]);
            Assert.Equal("value2", result[0].Constants["key2"]);

            // Clean up
            File.Delete(this.testConstantsYamlFilePath);
        }

        [Fact]
        public async Task ReadConstantsYamlFileAsync_ThrowsFileNotFoundException_ForNonexistentFile()
        {
            // Arrange
            var nonExistentFilePath = Path.Combine(this.oryxRootPath, "non-existent.yaml");

            // Act and assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                this.yamlFileService.ReadConstantsYamlFileAsync(nonExistentFilePath));

            Assert.Contains("YAML file not found.", ex.Message);
        }

        [Fact]
        public async Task ReadConstantsYamlFileAsync_ShouldThrowYamlException_WhenYamlFileHasInvalidFormat()
        {
            // Arrange
            var yamlFileService = new YamlFileService(this.oryxRootPath);
            var invalidYamlFilePath = Path.Combine(this.oryxRootPath, "invalid-constants.yaml");

            // Write an invalid YAML file
            await File.WriteAllTextAsync(invalidYamlFilePath, "invalid: : : yaml");

            // Act and assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await yamlFileService.ReadConstantsYamlFileAsync(invalidYamlFilePath));

            // Cleanup
            File.Delete(invalidYamlFilePath);
        }

        [Fact]
        public void WriteConstantsYamlFile_WritesYamlContentsToGivenFilePath()
        {
            // Arrange
            string filePath = "test_constants.yaml";
            var yamlConstants = new List<ConstantsYamlFile>
            {
                new ConstantsYamlFile
                {
                    Name =  "test",
                    Constants = new Dictionary<string, object>
                    {
                        {"key1", "value1"},
                        {"key2", "value2"}
                    }
                }
            };

            // Act
            this.yamlFileService.WriteConstantsYamlFile(filePath, yamlConstants);

            // Assert
            Assert.Equal("- name: test\r\n  constants:\r\n    key1: value1\r\n    key2: value2\r\n  outputs: []\r\n", File.ReadAllText(filePath));

            // Clean up
            File.Delete(filePath);
        }
    }
}
