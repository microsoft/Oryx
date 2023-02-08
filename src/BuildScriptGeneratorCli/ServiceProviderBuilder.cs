// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.ApplicationInsights;
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
        private IServiceCollection serviceCollection;

        public ServiceProviderBuilder(string logFilePath = null, IConsole console = null)
        {
            this.serviceCollection = new ServiceCollection();
            var connectionString = string.Empty;
            this.serviceCollection
                .AddBuildScriptGeneratorServices()
                .AddCliServices(console)
                .AddLogging(builder =>
                {
                    builder.AddApplicationInsights(
                         configureTelemetryConfiguration: (config) => config.ConnectionString = connectionString,
                         configureApplicationInsightsLoggerOptions: (options) => { });
                    builder.SetMinimumLevel(Extensions.Logging.LogLevel.Trace);
                })
                .AddSingleton<TelemetryClient>(new TelemetryClient(new ApplicationInsights.Extensibility.TelemetryConfiguration
                {
                    ConnectionString = connectionString,
                }));
        }

        public ServiceProviderBuilder ConfigureServices(Action<IServiceCollection> configure)
        {
            configure(this.serviceCollection);
            return this;
        }

        public ServiceProviderBuilder ConfigureScriptGenerationOptions(Action<BuildScriptGeneratorOptions> configure)
        {
            this.serviceCollection.Configure<BuildScriptGeneratorOptions>(opts => configure(opts));
            return this;
        }

        public IServiceProvider Build()
        {
            return this.serviceCollection.BuildServiceProvider();
        }
    }
}