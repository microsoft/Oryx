// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGenerator.Tests
{
    public class DefaultSourceRepoProviderTest : IClassFixture<TestTempDirTestFixture>
    {
        private readonly string _tempDirRootPath;

        public DefaultSourceRepoProviderTest(TestTempDirTestFixture fixture)
        {
            _tempDirRootPath = fixture.RootDirPath;
        }

        [Fact]
        public void GetSourceRepo_ReturnsSameInstance_OnMultipleCalls()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var appDir = Path.Combine(_tempDirRootPath, $"app-{guid}");
            Directory.CreateDirectory(appDir);
            var options = new BuildScriptGeneratorOptions
            {
                SourceDir = appDir,
            };
            var provider = GetSourceRepoProvider(options);

            // Act-1
            var sourceRepo1 = provider.GetSourceRepo();

            // Assert-1
            Assert.Equal(appDir, sourceRepo1.RootPath);

            // Act-2
            var sourceRepo2 = provider.GetSourceRepo();

            // Assert-2
            Assert.Equal(appDir, sourceRepo2.RootPath);
            Assert.Same(sourceRepo1, sourceRepo2);
        }

        [Fact]
        public void IntermediateDir_IsNotUsed_ByDefault()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var appDir = Path.Combine(_tempDirRootPath, $"app-{guid}");
            Directory.CreateDirectory(appDir);
            var options = new BuildScriptGeneratorOptions
            {
                SourceDir = appDir,
            };
            var provider = GetSourceRepoProvider(options);

            // Act
            var sourceRepo = provider.GetSourceRepo();

            // Assert
            Assert.Equal(appDir, sourceRepo.RootPath);
        }

        private ISourceRepoProvider GetSourceRepoProvider(BuildScriptGeneratorOptions options)
        {
            return new DefaultSourceRepoProvider(Options.Create(options), NullLoggerFactory.Instance);
        }

        private class TestTempDirectoryProvider : ITempDirectoryProvider
        {
            private readonly string _tempDir;

            public TestTempDirectoryProvider(string tempDir)
            {
                _tempDir = tempDir;
            }

            public string GetTempDirectory()
            {
                Directory.CreateDirectory(_tempDir);
                return _tempDir;
            }
        }
    }
}