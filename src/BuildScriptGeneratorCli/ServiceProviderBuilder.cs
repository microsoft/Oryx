// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------
namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Oryx.BuildScriptGenerator;

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
                options.SourcePath = Path.GetFullPath(program.SourceCodeFolder);
                options.TargetScriptPath = program.TargetScriptPath;
            });
            return this;
        }

        public IServiceProvider Build()
        {
            return _serviceCollection.BuildServiceProvider();
        }

        private static IConfiguration GetConfiguration()
        {
            // The order of 'Add' is important here.
            // The values provided at commandline override any values provided in environment variables
            // and values provided in environment variables override any values provided in appsettings.json.
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(path: "appsettings.json", optional: true)
                .AddEnvironmentVariables();

            return configurationBuilder.Build();
        }
    }
}