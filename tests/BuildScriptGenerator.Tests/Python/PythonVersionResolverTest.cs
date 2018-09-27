// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Oryx.BuildScriptGenerator.Python;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class PythonVersionResolverTest
    {
        [Fact]
        public void SimpleSupportedPythonVersionSpecified()
        {
            // Arrange
            const string version = "3.7.0";
            var supportedVersions = new[] { version };
            var pythonVersionResolver = CreatePythonVersionResolver(supportedVersions);

            // Act
            var returnedVersion = pythonVersionResolver.GetSupportedPythonVersion("3.7");

            // Assert;
            Assert.Equal(version, returnedVersion);
        }

        [Fact]
        public void SimpleUnsupportedPythonVersionSpecified()
        {
            // Arrange
            const string supportedVersion = "3.7.0";
            var supportedVersions = new[] { supportedVersion };
            var pythonVersionResolver = CreatePythonVersionResolver(supportedVersions);

            // Act
            var returnedVersion = pythonVersionResolver.GetSupportedPythonVersion("3.7.1");

            // Assert;
            Assert.Null(returnedVersion);
        }

        [Fact]
        public void MinimumPythonVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "3.6.6", "3.7.0" };
            var pythonVersionResolver = CreatePythonVersionResolver(supportedVersions);

            // Act
            var returnedVersion = pythonVersionResolver.GetSupportedPythonVersion(">=3.6.6");

            // Assert;
            Assert.Equal("3.7.0", returnedVersion);
        }

        [Fact]
        public void SpecificPythonVersionSpecified()
        {
            // Arrange
            var supportedVersions = new[] { "3.6.6", "3.7.0" };
            var pythonVersionResolver = CreatePythonVersionResolver(supportedVersions);

            // Act
            var returnedVersion = pythonVersionResolver.GetSupportedPythonVersion("=3.6.6");

            // Assert;
            Assert.Equal("3.6.6", returnedVersion);
        }

        private IPythonVersionResolver CreatePythonVersionResolver(string[] supportedVersions)
        {
            var pythonVersionProvider = new TestPythonVersionProvider(supportedVersions);

            return new PythonVersionResolver(pythonVersionProvider, NullLogger<PythonVersionResolver>.Instance);
        }

        private class TestPythonVersionProvider : IPythonVersionProvider
        {
            public TestPythonVersionProvider(string[] supportedVersions)
            {
                SupportedPythonVersions = supportedVersions;
            }

            public IEnumerable<string> SupportedPythonVersions { get; }
        }
    }
}