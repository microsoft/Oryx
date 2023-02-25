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
        private readonly YamlFileService _yamlFileService;

        public YamlFileServiceTests()
        {
            _yamlFileService = new YamlFileService();
        }

        [Fact]
        public async Task ReadConstantsYamlFileAsync_ReturnsExpectedYamlContents()
        {
            // Arrange
            string filePath = "test_constants.yaml";
            string yamlContents = "- name: test\r\n  constants:\r\n    key1: value1\r\n    key2: value2";
            File.WriteAllText(filePath, yamlContents);

            // Act
            var result = await _yamlFileService.ReadConstantsYamlFileAsync(filePath);

            // Assert
            Assert.Single(result);
            Assert.Equal("value1", result[0].Constants["key1"]);
            Assert.Equal("value2", result[0].Constants["key2"]);

            // Clean up
            File.Delete(filePath);
        }

        [Fact]
        public async Task ReadConstantsYamlFileAsync_ThrowsFileNotFoundException_ForNonexistentFile()
        {
            // Arrange
            string filePath = "nonexistent.yaml";

            // Act and assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _yamlFileService.ReadConstantsYamlFileAsync(filePath));

            Assert.Contains("YAML file not found.", ex.Message);
        }

        [Fact]
        public async Task ReadConstantsYamlFileAsync_ThrowsYamlException_ForInvalidYaml()
        {
            // Arrange
            string filePath = "invalid_constants.yaml";
            string invalidYamlContents = "invalid-yaml-contents";
            File.WriteAllText(filePath, invalidYamlContents);

            // Act and assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _yamlFileService.ReadConstantsYamlFileAsync(filePath));

            Assert.Contains("Invalid YAML file format.", ex.Message);

            // Clean up
            File.Delete(filePath);
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
            _yamlFileService.WriteConstantsYamlFile(filePath, yamlConstants);

            // Assert
            Assert.Equal("- name: test\r\n  constants:\r\n    key1: value1\r\n    key2: value2\r\n", File.ReadAllText(filePath));

            // Clean up
            File.Delete(filePath);
        }
    }
}
