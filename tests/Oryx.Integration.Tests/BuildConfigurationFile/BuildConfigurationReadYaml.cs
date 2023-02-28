using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.BuildScriptGenerator.Resources;
using Microsoft.Oryx.Tests.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Oryx.Integration.Tests.BuildConfigurationFile
{
    public class BuildConfigurationReadYaml : IClassFixture<TestTempDirTestFixture>
    {     
        private string GenerateScript(BuildConfigurationFileFlags flags)//
        {
            // Arrange
            var detector1 = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "lang1",
                detectedPlatformVersion: "1.0.0");
            var platform1 = new TestProgrammingPlatform(
                "lang1",
                new[] { "1.0.0" },
                canGenerateScript: false,
                scriptContent: null,
                installationScriptContent: "lang1-installationscript",
                detector: detector1);
            var detector2 = new TestPlatformDetectorUsingPlatformName(
                detectedPlatformName: "lang2",
                detectedPlatformVersion: "1.0.0");
            var platform2 = new TestProgrammingPlatform(
                "lang2",
                new[] { "1.0.0" },
                canGenerateScript: true,
                installationScriptContent: "lang2-installationscript",
                scriptContent: "script-content",
                detector: detector2);

            var commonOptions = new BuildScriptGeneratorOptions
            {
                PlatformName = "lang2",
                PlatformVersion = "1.0.0",
            };
            var generator = CreateDefaultScriptGenerator(new[] { platform1, platform2 }, commonOptions);
            var context = CreateScriptGeneratorContext();

            ((TestSourceRepo)context.SourceRepo).buildConfigurationFileFlags = flags;
            // Act
            generator.GenerateBashScript(context, out var generatedScript);
            return generatedScript;
           
        }

        [Fact]
        public void UsingBuildConfigurationFile_PreBuildOnly_Valid_ReturnsTrue()
        {
            string generatedScript = GenerateScript(BuildConfigurationFileFlags.PreBuild);
            Assert.Contains("apt-get install xyz", generatedScript);
        }

        [Fact]
        public void UsingBuildConfigurationFile_PreBuildOnly_Multiline_Valid_ReturnsTrue()
        {
            string generatedScript = GenerateScript(BuildConfigurationFileFlags.PreBuild | BuildConfigurationFileFlags.MultiLine);
            Assert.Contains("apt-get install abc", generatedScript);
        }

        [Fact]
        public void UsingBuildConfigurationFile_PreBuildOnly_Invalid_ReturnsTrue()
        {
            string generatedScript = GenerateScript(BuildConfigurationFileFlags.PreBuild | BuildConfigurationFileFlags.Invalid);
            Assert.DoesNotContain("pre-biuld: ", generatedScript);
        }

        [Fact]
        public void UsingBuildConfigurationFile_PostBuildOnly_Valid_ReturnsTrue()
        {
            string generatedScript = GenerateScript(BuildConfigurationFileFlags.PostBuild);
            Assert.Contains("python manage.py makemigrations", generatedScript);
        }

        [Fact]
        public void UsingBuildConfigurationFile_PostBuildOnly_Multiline_Valid_ReturnsTrue()
        {
            string generatedScript = GenerateScript(BuildConfigurationFileFlags.PostBuild | BuildConfigurationFileFlags.MultiLine);
            Assert.Contains("python manage.py migrate", generatedScript);
        }

        [Fact]
        public void UsingBuildConfigurationFile_PostBuildOnly_Invalid_ReturnsTrue()
        {
            string generatedScript = GenerateScript(BuildConfigurationFileFlags.PostBuild | BuildConfigurationFileFlags.Invalid);
            Assert.DoesNotContain("post-build ", generatedScript);
        }

        private DefaultBuildScriptGenerator CreateDefaultScriptGenerator(
            IProgrammingPlatform platform,
            BuildScriptGeneratorOptions commonOptions)
        {
            return CreateDefaultScriptGenerator(new[] { platform }, commonOptions, checkers: null);
        }

        private DefaultCompatiblePlatformDetector CreateDefaultCompatibleDetector(
            IProgrammingPlatform[] platforms,
            BuildScriptGeneratorOptions commonOptions)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            commonOptions.SourceDir = "/app";
            commonOptions.DestinationDir = "/output";

            var configuration = new TestConfiguration();
            configuration[$"{commonOptions.PlatformName}_version"] = commonOptions.PlatformVersion;
            return new DefaultCompatiblePlatformDetector(
                    platforms,
                    NullLogger<DefaultCompatiblePlatformDetector>.Instance,
                    Options.Create(commonOptions));
        }

        private DefaultBuildScriptGenerator CreateDefaultScriptGenerator(
            IProgrammingPlatform[] platforms,
            BuildScriptGeneratorOptions commonOptions,
            IEnumerable<IChecker> checkers = null)
        {
            commonOptions = commonOptions ?? new BuildScriptGeneratorOptions();
            commonOptions.SourceDir = "/app";
            commonOptions.DestinationDir = "/output";

            var defaultPlatformDetector = new DefaultPlatformsInformationProvider(
                platforms,
                new DefaultStandardOutputWriter());
            var envScriptProvider = new PlatformsInstallationScriptProvider(
                platforms,
                defaultPlatformDetector,
                new DefaultStandardOutputWriter());
            return new DefaultBuildScriptGenerator(
                defaultPlatformDetector,
                envScriptProvider,
                Options.Create(commonOptions),
                new DefaultCompatiblePlatformDetector(
                    platforms,
                    NullLogger<DefaultCompatiblePlatformDetector>.Instance,
                    Options.Create(commonOptions)),
                checkers,
                NullLogger<DefaultBuildScriptGenerator>.Instance,
                new DefaultStandardOutputWriter(), 
                TelemetryClientHelper.GetTelemetryClient()); 
        }

        private static BuildScriptGeneratorContext CreateScriptGeneratorContext()
        {
            return new BuildScriptGeneratorContext
            {
                SourceRepo = new TestSourceRepo(),
            };
        }

        [Flags]
        public enum BuildConfigurationFileFlags
        {
            Empty = 1,
            PreBuild = 2,
            Build = 4,
            PostBuild = 8,
            Run = 16,
            CRLF = 32,
            MultiLine = 64,
            Invalid = 128,
            Missing = 256
        }

        private class TestSourceRepo : ISourceRepo
        {
            public string RootPath => string.Empty;
            public BuildConfigurationFileFlags buildConfigurationFileFlags;
            

            public bool FileExists(params string[] paths)
            {
                return !buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.Missing);
            }

            public bool DirExists(params string[] paths)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<string> EnumerateFiles(string searchPattern, bool searchSubDirectories)
            {
                throw new NotImplementedException();
            }

            public string ReadFile(params string[] paths)
            {
                System.Text.StringBuilder text = new System.Text.StringBuilder();
                if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.Empty))
                    if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.Invalid))
                        return null;
                    else return string.Empty;
                else
                {
                    //
                    // YAML file Header
                    //
                    if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.CRLF))
                    {
                        text.Append("version: 1\r\n");
                    }
                    else
                    {
                        text.Append("version: 1\n");
                    }

                    //
                    // Pre-build
                    //
                    if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.PreBuild))
                    {
                        if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.Invalid))
                        {
                            text.Append("pre-biuld: ");
                        }
                        else
                        {
                            text.Append("pre-build: ");
                        }
                        if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.MultiLine))
                        {
                            if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.CRLF))
                            {
                                text.Append("|\r\n  apt-get install xyz\r\n  apt-get install abc");
                            }
                            else
                            {
                                text.Append("|\n  apt-get install xyz\n  apt-get install abc");
                            }
                          
                        }
                        else
                        {
                            text.Append("apt-get install xyz");
                        }
                        
                    }

                    //
                    // Post-build
                    //
                    if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.PostBuild))
                    {
                        if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.Invalid))
                        {
                            text.Append("post-build ");
                        }
                        else
                        {
                            text.Append("post-build: ");
                        }
                        if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.MultiLine))
                        {
                            if (buildConfigurationFileFlags.HasFlag(BuildConfigurationFileFlags.CRLF))
                            {
                                text.Append("|\r\n  python manage.py makemigrations\r\n    python manage.py migrate");
                            }
                            else
                            {
                                text.Append("|\n  python manage.py makemigrations\n    python manage.py migrate");
                            }
                        }
                        else
                        {
                            text.Append("python manage.py makemigrations");
                        }
                    }
                    return text.ToString();
                }
            }

            public string[] ReadAllLines(params string[] paths)
            {
                throw new NotImplementedException();
            }

            public string GetGitCommitId() => null;
        }
    }

    public class TestConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string> _values;

        public TestConfiguration()
        {
            // Since actual instance of ASP.NET Core's configuration is case-insensitive, make it case-insensitive
            // here also.
            _values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string this[string key]
        {
            get
            {
                if (_values.ContainsKey(key))
                {
                    return _values[key];
                }
                return string.Empty;
            }
            set
            {
                _values[key] = value;
            }
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new NotImplementedException();
        }

        public IChangeToken GetReloadToken()
        {
            throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key)
        {
            throw new NotImplementedException();
        }
    }
}
