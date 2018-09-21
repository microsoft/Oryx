// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    /// <summary>
    /// Configures the service provider, where all dependency injection is setup.
    /// </summary>
    internal class ServiceProviderBuilder
    {
        private IServiceCollection _serviceCollection;

        public ServiceProviderBuilder()
        {
            var configuration = GetConfiguration();

            _serviceCollection = new ServiceCollection();
            _serviceCollection
                .AddBuildScriptGeneratorServices()
                .AddLogging(loggingBuilder =>
                {
                    loggingBuilder
                    .AddConfiguration(configuration.GetSection("Logging"))
                    .AddConsole()
                    .AddDebug();
                });
        }

        public ServiceProviderBuilder WithScriptGenerationOptions(Program program)
        {
            _serviceCollection.Configure<BuildScriptGeneratorOptions>(options =>
            {
                options.SourceCodeFolder = Path.GetFullPath(program.SourceCodeFolder);
                options.ScriptPath = program.ScriptPath;
                options.ScriptOnly = program.ScriptOnly;
                options.LanguageName = program.LanguageName;
                options.LanguageVersion = program.LanguageVersion;

                if (!string.IsNullOrEmpty(program.OutputFolder))
                {
                    options.OutputFolder = Path.GetFullPath(program.OutputFolder);
                }

                if (!string.IsNullOrEmpty(program.IntermediateFolder))
                {
                    options.IntermediateFolder = Path.GetFullPath(program.IntermediateFolder);
                }

                // Create one unique subdirectory per session (or run of this tool)
                // Example structure:
                // /tmp/BuildScriptGenerator/guid1
                // /tmp/BuildScriptGenerator/guid2
                options.TempDirectory = Path.Combine(
                    Path.GetTempPath(),
                    nameof(BuildScriptGenerator),
                    Guid.NewGuid().ToString("N"));
            });
            return this;
        }

        public IServiceProvider Build()
        {
            return _serviceCollection.BuildServiceProvider();
        }

        private static IConfiguration GetConfiguration()
        {
            var executingAssemblyFileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var basePath = executingAssemblyFileInfo.Directory.FullName;

            // The order of 'Add' is important here.
            // The values provided at commandline override any values provided in environment variables
            // and values provided in environment variables override any values provided in appsettings.json.
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .SetBasePath(basePath)
                .AddJsonFile(path: "appsettings.json", optional: true)
                .AddEnvironmentVariables();

            return configurationBuilder.Build();
        }
    }
}