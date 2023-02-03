// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using JetBrains.Annotations;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Common.Extensions;
using NLog.Config;
using NLog.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    /// <summary>
    /// Configures the service provider, where all dependency injection is setup.
    /// </summary>
    internal class ServiceProviderBuilder
    {
        private readonly IServiceCollection serviceCollection;

        public ServiceProviderBuilder(string logFilePath = null, IConsole console = null)
        {
            this.serviceCollection = new ServiceCollection();
            this.serviceCollection
                .AddBuildScriptGeneratorServices()
                .AddCliServices(console)
                .AddLogging(builder =>
                {
                    builder.AddApplicationInsights(
                         configureTelemetryConfiguration: (config) => config.ConnectionString = " ",
                         configureApplicationInsightsLoggerOptions: (options) => { });
                    builder.SetMinimumLevel(Extensions.Logging.LogLevel.Trace);
                    builder.AddNLog(new NLogProviderOptions
                    {
                        CaptureMessageTemplates = true,
                        CaptureMessageProperties = true,
                    });
                })
                .AddSingleton<ITelemetryClientExtension>(new TelemetryClientExtension(" "));
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

            var disableTelemetryEnvVariableValue = Environment.GetEnvironmentVariable(
                LoggingConstants.OryxDisableTelemetryEnvironmentVariableName);
            _ = bool.TryParse(disableTelemetryEnvVariableValue, out bool disableTelemetry);

            var aiKey = disableTelemetry ? string.Empty : Environment.GetEnvironmentVariable(
                LoggingConstants.ApplicationInsightsInstrumentationKeyEnvironmentVariableName);
            if (!string.IsNullOrWhiteSpace(aiKey))
            {
                var aiTarget = new ApplicationInsights.NLogTarget.ApplicationInsightsTarget()
                {
                    Name = "ai",
                    InstrumentationKey = aiKey,
                };
                config.AddTarget(aiTarget);
                config.AddRuleForAllLevels(aiTarget);
            }

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