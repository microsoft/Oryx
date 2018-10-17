using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGeneratorCli;
using Xunit;

namespace BuildScriptGeneratorCli.Tests
{
    public class LanguagesCommandTest
    {
        // Use different 'type' for each test script generator as otherwise the 'TryAdd' would ignore
        // if the implementation type is the same one. Here we use generics to make them look as 
        // different types.

        [Fact]
        public void SortsLanguagesByName_InAscendingOrder()
        {
            // Arrange
            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureServices(services =>
                {
                    services.RemoveAll<IScriptGenerator>();
                    services.TryAddEnumerable(new[]
                    {
                        ServiceDescriptor.Singleton<IScriptGenerator>(
                            new TestScriptGenerator<string>("d", new[]{ "1.0.0" })),
                        ServiceDescriptor.Singleton<IScriptGenerator>(
                            new TestScriptGenerator<int>("c", new[]{ "1.0.0" })),
                        ServiceDescriptor.Singleton<IScriptGenerator>(
                            new TestScriptGenerator<decimal>("b", new[]{ "1.0.0" }))
                    });
                })
                .Build();
            var testConsole = new TestConsole();
            var languagesCommand = new LanguagesCommand();

            // Act
            var exitCode = languagesCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Empty(testConsole.StdError);
            var lines = testConsole.StdOutput.Split(Environment.NewLine);
            Assert.Collection(
                lines,
                (line) => Assert.StartsWith("b", line),
                (line) => Assert.StartsWith("c", line),
                (line) => Assert.StartsWith("d", line),
                (line) => Assert.Empty(line));
        }

        [Fact]
        public void SortsLanguagesByName_InAscendingOrder_IgnoringCase()
        {
            // Arrange
            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureServices(services =>
                {
                    services.RemoveAll<IScriptGenerator>();
                    services.TryAddEnumerable(new[]
                    {
                        ServiceDescriptor.Singleton<IScriptGenerator>(
                            new TestScriptGenerator<string>("D", new[]{ "1.0.0" })),
                        ServiceDescriptor.Singleton<IScriptGenerator>(
                            new TestScriptGenerator<int>("c", new[]{ "1.0.0" })),
                        ServiceDescriptor.Singleton<IScriptGenerator>(
                            new TestScriptGenerator<decimal>("B", new[]{ "1.0.0" }))
                    });
                })
                .Build();
            var testConsole = new TestConsole();
            var languagesCommand = new LanguagesCommand();

            // Act
            var exitCode = languagesCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Empty(testConsole.StdError);
            var lines = testConsole.StdOutput.Split(Environment.NewLine);
            Assert.Collection(
                lines,
                (line) => Assert.StartsWith("B", line),
                (line) => Assert.StartsWith("c", line),
                (line) => Assert.StartsWith("D", line),
                (line) => Assert.Empty(line));
        }

        [Fact]
        public void SortsVersionsInSemanticOrder()
        {
            // Arrange
            var serviceProvider = new ServiceProviderBuilder()
                .ConfigureServices(services =>
                {
                    services.RemoveAll<IScriptGenerator>();
                    services.TryAddEnumerable(new[]
                    {
                        ServiceDescriptor.Singleton<IScriptGenerator>(
                            new TestScriptGenerator<string>(
                                "lang1", 
                                new[]{ "11.0.0", "8.11.2", "8.4.2", "6.5.3", "6.4.1" }))
                    });
                })
                .Build();
            var testConsole = new TestConsole();
            var languagesCommand = new LanguagesCommand();

            // Act
            var exitCode = languagesCommand.Execute(serviceProvider, testConsole);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.Empty(testConsole.StdError);
            var lines = testConsole.StdOutput.Split(Environment.NewLine);
            Assert.Collection(
                lines,
                (line) => Assert.StartsWith("lang1: 6.4.1, 6.5.3, 8.4.2, 8.11.2, 11.0.0", line),
                (line) => Assert.Empty(line));
        }

        private class TestScriptGenerator<T> : IScriptGenerator
        {
            public TestScriptGenerator(string languageName, string[] languageVersions)
            {
                SupportedLanguageName = languageName;
                SupportedLanguageVersions = languageVersions;
            }

            public string SupportedLanguageName { get; }

            public IEnumerable<string> SupportedLanguageVersions { get; }

            public bool CanGenerateScript(ScriptGeneratorContext scriptGeneratorContext)
            {
                throw new NotImplementedException();
            }

            public string GenerateBashScript(ScriptGeneratorContext scriptGeneratorContext)
            {
                throw new NotImplementedException();
            }
        }
    }
}
