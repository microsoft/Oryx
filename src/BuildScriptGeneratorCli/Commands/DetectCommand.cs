// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGenerator.Common.Extensions;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;
using Microsoft.Oryx.Detector;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class DetectCommand : CommandBase
    {
        public const string Name = "detect";
        public const string Description = "Detect all platforms and versions in the given app source directory.";

        public DetectCommand()
        {
        }

        public DetectCommand(DetectCommandProperty input)
        {
            this.SourceDir = input.SourceDir;
            this.Platform = input.Platform;
            this.OutputFormat = input.OutputFormat;
            this.LogFilePath = input.LogPath;
            this.DebugMode = input.DebugMode;
        }

        public string SourceDir { get; set; }

        public string Platform { get; set; }

        public string OutputFormat { get; set; }

        public static Command Export(IConsole console)
        {
            var sourceDirArgument = new Argument<string>(
                name: OptionArgumentTemplates.SourceDir,
                description: OptionArgumentTemplates.DetectSourceDirDescription,
                getDefaultValue: () => Directory.GetCurrentDirectory());
            var platformOption = new Option<string>(
                name: OptionArgumentTemplates.Platform,
                description: OptionArgumentTemplates.PlatformDescription);
            var outputFormatOption = new Option<string>(
                aliases: OptionArgumentTemplates.Output,
                description: OptionArgumentTemplates.DetectOutputDescription);
            var logFilePathOption = new Option<string>(OptionArgumentTemplates.Log, OptionArgumentTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionArgumentTemplates.Debug, OptionArgumentTemplates.DebugDescription);

            var command = new Command(Name, Description)
            {
                sourceDirArgument,
                platformOption,
                outputFormatOption,
                logFilePathOption,
                debugOption,
            };

            command.SetHandler(
                (prop) =>
                {
                    var detectCommand = new DetectCommand(prop);
                    return Task.FromResult(detectCommand.OnExecute(console));
                },
                new DetectCommandBinder(
                    sourceDir: sourceDirArgument,
                    platform: platformOption,
                    outputFormat: outputFormatOption,
                    logPath: logFilePathOption,
                    debugMode: debugOption));

            return command;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();
            var logger = loggerFactory.CreateLogger<DetectCommand>();
            var sourceRepo = new LocalSourceRepo(this.SourceDir, loggerFactory);
            var ctx = new DetectorContext
            {
                SourceRepo = sourceRepo,
            };

            var detector = serviceProvider.GetRequiredService<IDetector>();

            using (var timedEvent = telemetryClient.LogTimedEvent("DetectCommand"))
            {
                // Try to only detect a single platform, if one was provided
                if (!string.IsNullOrEmpty(this.Platform))
                {
                    var detectedPlatformResult = detector.GetDetectedPlatform(ctx, this.Platform);
                    PrintJsonResult(detectedPlatformResult, console);
                    return ProcessConstants.ExitSuccess;
                }

                var detectedPlatformResults = detector.GetAllDetectedPlatforms(ctx);

                if (detectedPlatformResults == null || !detectedPlatformResults.Any())
                {
                    logger?.LogError($"No platforms and versions detected from source directory: '{this.SourceDir}'");
                    console.WriteErrorLine($"No platforms and versions detected from source directory: '{this.SourceDir}'");
                }

                if (!string.IsNullOrEmpty(this.OutputFormat)
                    && string.Equals(this.OutputFormat, "json", StringComparison.OrdinalIgnoreCase))
                {
                    PrintJsonResult(detectedPlatformResults, console);
                }
                else
                {
                    PrintTableResult(detectedPlatformResults, console);
                }

                return ProcessConstants.ExitSuccess;
            }
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<DetectCommand>>();
            this.SourceDir = string.IsNullOrEmpty(this.SourceDir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(this.SourceDir);

            if (!Directory.Exists(this.SourceDir))
            {
                logger?.LogError("Could not find the source directory.");
                console.WriteErrorLine($"Could not find the source directory: '{this.SourceDir}'");

                return false;
            }

            if (!string.IsNullOrEmpty(this.OutputFormat)
                && !string.Equals(this.OutputFormat, "json", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(this.OutputFormat, "table", StringComparison.OrdinalIgnoreCase))
            {
                logger?.LogError("Unsupported output format. Supported output formats are: json, table.");
                console.WriteErrorLine($"Unsupported output format: '{this.OutputFormat}'. " +
                    "Supported output formats are: json, table.");

                return false;
            }

            var validPlatforms = new List<string> { "dotnet", "node", "python", "java", "php", "go", "ruby" };
            if (!string.IsNullOrEmpty(this.Platform) && !validPlatforms.Contains(this.Platform.ToLower()))
            {
                logger?.LogError($"Unsupported platform provided. Supported platforms are {string.Join(", ", validPlatforms)}");
                console.WriteErrorLine($"Unsupported platform provided. Supported platforms are {string.Join(", ", validPlatforms)}");
                return false;
            }

            return true;
        }

        private static void PrintTableResult(IEnumerable<PlatformDetectorResult> detectedPlatformResults, IConsole console)
        {
            var defs = new DefinitionListFormatter();
            if (detectedPlatformResults == null || !detectedPlatformResults.Any())
            {
                defs.AddDefinition("Platform", "Not Detected");
                defs.AddDefinition("PlatformVersion", "Not Detected");
                console.WriteLine(defs.ToString());
                return;
            }

            foreach (var detectedPlatformResult in detectedPlatformResults)
            {
                // This is to filter out the indexed properties from properties variable.
                var propertyInfos = detectedPlatformResult
                    .GetType()
                    .GetProperties()
                    .Where(p => p.GetIndexParameters().Length == 0);

                // Get all properties from a detected platform result and add them to DefinitionListFormatter.
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = propertyInfo.GetValue(detectedPlatformResult, null);
                    var propertyString = string.Empty;
                    if (propertyValue is IEnumerable && !(propertyValue is string))
                    {
                        propertyString = string.Join(", ", (propertyValue as IEnumerable<FrameworkInfo>).Select(x => x.ToString()));
                    }
                    else
                    {
                        propertyString = propertyValue == null ? "Not Detected" : propertyValue.ToString();
                    }

                    defs.AddDefinition(propertyInfo.Name, propertyString);
                }
            }

            console.WriteLine(defs.ToString());
        }

        private static void PrintJsonResult(PlatformDetectorResult detectedPlatformResult, IConsole console)
        {
            if (detectedPlatformResult == null)
            {
                console.WriteLine("{}");
                return;
            }

            detectedPlatformResult.PlatformVersion = detectedPlatformResult.PlatformVersion ?? string.Empty;

            console.WriteLine(JsonConvert.SerializeObject(detectedPlatformResult, Formatting.Indented));
        }

        private static void PrintJsonResult(IEnumerable<PlatformDetectorResult> detectedPlatformResults, IConsole console)
        {
            if (detectedPlatformResults == null || !detectedPlatformResults.Any())
            {
                console.WriteLine("{}");
                return;
            }

            foreach (var detectedPlatformResult in detectedPlatformResults)
            {
                detectedPlatformResult.PlatformVersion = detectedPlatformResult.PlatformVersion ?? string.Empty;
            }

            console.WriteLine(JsonConvert.SerializeObject(detectedPlatformResults, Formatting.Indented));
        }
    }
}
