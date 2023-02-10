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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.BuildScriptGeneratorCli.Commands;
using Microsoft.Oryx.Detector;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal class DetectCommand : CommandBase
    {
        public DetectCommand()
        {
        }

        public DetectCommand(DetectCommandProperty input)
        {
            this.SourceDir = input.SourceDir;
            this.OutputFormat = input.OutputFormat;
            this.LogFilePath = input.LogFilePath;
            this.DebugMode = input.DebugMode;
        }

        public string SourceDir { get; set; }

        public string OutputFormat { get; set; }

        public static Command Export()
        {
            var sourceDirArgument = new Argument<string>("sourceDirArg", "The source directory. If no value is provided, the current directory is used.");
            var outputFormatOption = new Option<string>(
                aliases: new[] { "-o", "--output" },
                description: "Output the detected platform data in chosen format. " +
                             "Example: json, table. " +
                             "If not set, by default output will print out as a table. ");
            var logFilePathOption = new Option<string>(OptionTemplates.Log, OptionTemplates.LogDescription);
            var debugOption = new Option<bool>(OptionTemplates.Debug, OptionTemplates.DebugDescription);

            var command = new Command("detect", "Detect all platforms and versions in the given app source directory.")
            {
                sourceDirArgument,
                outputFormatOption,
                logFilePathOption,
                debugOption,
            };

            command.SetHandler(
                (prop) =>
                {
                    var detectCommand = new DetectCommand(prop);
                    detectCommand.OnExecute();
                },
                new DetectCommandBinder(
                    sourceDirArgument,
                    outputFormatOption,
                    logFilePathOption,
                    debugOption));

            return command;
        }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<DetectCommand>();
            var sourceRepo = new LocalSourceRepo(this.SourceDir, loggerFactory);
            var ctx = new DetectorContext
            {
                SourceRepo = sourceRepo,
            };

            var detector = serviceProvider.GetRequiredService<IDetector>();
            var detectedPlatformResults = detector.GetAllDetectedPlatforms(ctx);
            using (var timedEvent = logger.LogTimedEvent("DetectCommand"))
            {
                if (detectedPlatformResults == null || !detectedPlatformResults.Any())
                {
                    logger?.LogError($"No platforms and versions detected from source directory: '{this.SourceDir}'");
                    console.Error.WriteLine($"No platforms and versions detected from source directory: '{this.SourceDir}'");
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
                console.Error.WriteLine($"Could not find the source directory: '{this.SourceDir}'");

                return false;
            }

            if (!string.IsNullOrEmpty(this.OutputFormat)
                && !string.Equals(this.OutputFormat, "json", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(this.OutputFormat, "table", StringComparison.OrdinalIgnoreCase))
            {
                logger?.LogError("Unsupported output format. Supported output formats are: json, table.");
                console.Error.WriteLine($"Unsupported output format: '{this.OutputFormat}'. " +
                    "Supported output formats are: json, table.");

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
