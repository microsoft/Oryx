// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using JetBrains.Annotations;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;

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
            LogManager.Configuration = BuildNLogConfiguration(logFilePath);
            LogManager.ReconfigExistingLoggers();

            var disableTelemetryEnvVariableValue = Environment.GetEnvironmentVariable(
               LoggingConstants.OryxDisableTelemetryEnvironmentVariableName);
            _ = bool.TryParse(disableTelemetryEnvVariableValue, out bool disableTelemetry);
            var config = new TelemetryConfiguration();
            var aiConnectionString = disableTelemetry ? null : Environment.GetEnvironmentVariable(
                LoggingConstants.ApplicationInsightsConnectionStringKeyEnvironmentVariableName);
            if (!string.IsNullOrWhiteSpace(aiConnectionString))
            {
                config.ConnectionString = aiConnectionString;
            }

            this.serviceCollection = new ServiceCollection();
            this.serviceCollection
                .AddBuildScriptGeneratorServices()
                .AddCliServices(console)
                .AddLogging(builder =>
                {
                    if (!string.IsNullOrWhiteSpace(aiConnectionString))
                    {
                        builder.AddApplicationInsights(
                            configureTelemetryConfiguration: (c) => c.ConnectionString = aiConnectionString,
                            configureApplicationInsightsLoggerOptions: (options) => { });
                    }

                    builder.SetMinimumLevel(Extensions.Logging.LogLevel.Trace);
                    builder.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageTemplates = true,
                        CaptureMessageProperties = true,
                    });
                })
                .AddSingleton<TelemetryClient>(new TelemetryClient(config));
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Cannot prematurely dispose of Application insights objects.")]
        private static LoggingConfiguration BuildNLogConfiguration([CanBeNull] string logPath)
        {
            var config = new LoggingConfiguration();
            bool hasLogPath = !string.IsNullOrWhiteSpace(logPath);
            if (hasLogPath || config.AllTargets.Count == 0)
            {
                if (!hasLogPath)
                {
                    logPath = LoggingConstants.DefaultLogPath;
                }

                // Default layout: "${longdate}|${level:uppercase=true}|${logger}|${message}"
                var fileTarget = new NLog.Targets.FileTarget("file")
                {
                    FileName = Path.GetFullPath(logPath),
                    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}${exception:format=ToString}",
                };
                config.AddTarget(fileTarget);
                config.AddRuleForAllLevels(fileTarget);
            }

            return config;
        }
    }
}