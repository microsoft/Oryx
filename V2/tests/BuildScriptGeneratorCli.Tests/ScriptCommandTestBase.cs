// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Tests.Common;
using Xunit;

namespace Microsoft.Oryx.BuildScriptGeneratorCli.Tests
{
    public class ScriptCommandTestBase : IClassFixture<TestTempDirTestFixture>
    {
        internal static TestTempDirTestFixture _testDir;
        internal static string _testDirPath;

        public ScriptCommandTestBase(TestTempDirTestFixture testFixture)
        {
            _testDir = testFixture;
            _testDirPath = testFixture.RootDirPath;
        }

        internal IServiceProvider CreateServiceProvider(TestProgrammingPlatform generator, bool scriptOnly)
        {
            var sourceCodeFolder = Path.Combine(_testDirPath, "src");
            Directory.CreateDirectory(sourceCodeFolder);
            var outputFolder = Path.Combine(_testDirPath, "output");
            Directory.CreateDirectory(outputFolder);
            var servicesBuilder = new ServiceProviderBuilder()
                .ConfigureServices(services =>
                {
                    // Add 'test' script generator here as we can control what the script output is rather
                    // than depending on in-built script generators whose script could change overtime causing
                    // this test to be difficult to manage.

                    services.RemoveAll<IPlatformDetector>();
                    services.TryAddEnumerable(
                        ServiceDescriptor.Singleton<IPlatformDetector>(
                            new TestPlatformDetectorUsingPlatformName(
                                detectedPlatformName: "test",
                                detectedPlatformVersion: "1.0.0")));

                    services.RemoveAll<IProgrammingPlatform>();
                    services.TryAddEnumerable(
                        ServiceDescriptor.Singleton<IProgrammingPlatform>(generator));

                    services.AddSingleton<ITempDirectoryProvider>(
                        new TestTempDirectoryProvider(Path.Combine(_testDirPath, "temp")));

                    var configuration = new ConfigurationBuilder().Build();
                    services.AddSingleton<IConfiguration>(configuration);
                })
                .ConfigureScriptGenerationOptions(o =>
                {
                    o.SourceDir = sourceCodeFolder;
                    o.DestinationDir = outputFolder;
                    o.ScriptOnly = scriptOnly;
                });
            return servicesBuilder.Build();
        }

        internal class TestTempDirectoryProvider : ITempDirectoryProvider
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
