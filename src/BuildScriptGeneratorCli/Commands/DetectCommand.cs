// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Common;
using Microsoft.Oryx.Detector;
using Newtonsoft.Json;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    [Command(Name, Description = "Detect all platforms and versions in the given app source directory.")]
    internal class DetectCommand : CommandBase
    {
        public const string Name = "detect";

        [Argument(0, Description = "The source directory. If no value is provided, the current directory is used.")]
        [DirectoryExists]
        public string SourceDir { get; set; }

        [Option(
            "--output",
            CommandOptionType.SingleValue,
            Description = "Output the detected platform data in chosen format. " +
            "Example: json, table. " +
            "If not set, by default output will print out as a table. ")]
        public string OutputFormat { get; set; }

        internal override int Execute(IServiceProvider serviceProvider, IConsole console)
        {
            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<DetectCommand>();
            var sourceRepo = new LocalSourceRepo(SourceDir, loggerFactory);
            var ctx = new DetectorContext
            {
                SourceRepo = sourceRepo,
            };

            var detector = serviceProvider.GetRequiredService<IDetector>();
            var detectedPlatforms = detector.GetAllDetectedPlatforms(ctx);
            using (var timedEvent = logger.LogTimedEvent("DetectCommand"))
            {
                if (detectedPlatforms == null || !detectedPlatforms.Any())
                {
                    logger?.LogError($"No platforms and versions detected from source directory: '{SourceDir}'");
                    console.WriteErrorLine($"No platforms and versions detected from source directory: '{SourceDir}'");
                }

                if (!string.IsNullOrEmpty(OutputFormat) && OutputFormat.Equals("json"))
                {
                    PrintJsonResult(detectedPlatforms, console);
                }
                else if (!string.IsNullOrEmpty(OutputFormat) && OutputFormat.Equals("table"))
                {
                    PrintTableResult(detectedPlatforms, console);
                }

                return ProcessConstants.ExitSuccess;
            }
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<DetectCommand>>();
            SourceDir = string.IsNullOrEmpty(SourceDir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(SourceDir);

            if (!Directory.Exists(SourceDir))
            {
                logger?.LogError("Could not find the source directory.");
                console.WriteErrorLine($"Could not find the source directory: '{SourceDir}'");

                return false;
            }

            return true;
        }

        private void PrintTableResult(IEnumerable<PlatformDetectorResult> detectedPlatforms, IConsole console)
        {
            var defs = new DefinitionListFormatter();
            if (detectedPlatforms == null || !detectedPlatforms.Any())
            {
                defs.AddDefinition("Platform", "Not Detected");
                defs.AddDefinition("Version", "Not Detected");
                console.WriteLine(defs.ToString());
                return;
            }

            var result = string.Empty;

            foreach (var detectedPlatform in detectedPlatforms)
            {

                defs.AddDefinition("Platform", detectedPlatform.Platform);
                defs.AddDefinition("Version", detectedPlatform.PlatformVersion ?? "Not Detected");
            }

            console.WriteLine(defs.ToString());
        }

        private void PrintJsonResult(IEnumerable<PlatformDetectorResult> detectedPlatforms, IConsole console)
        {
            var detectionResult = new Dictionary<string, string>
            {
                ["App_Path"] = SourceDir,
            };
            var platformData = new List<Dictionary<string, string>>();
            var jsonPlatformData = string.Empty;

            if (detectedPlatforms == null || !detectedPlatforms.Any())
            {
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    ["Platform_Name"] = "Not Detected",
                    ["Platform_Version"] = "Not Detected",
                };
                platformData.Add(data);
                jsonPlatformData = JsonConvert.SerializeObject(platformData, Formatting.Indented);
                detectionResult["Platform_Data"] = jsonPlatformData;
                console.WriteLine(JsonConvert.SerializeObject(detectionResult, Formatting.Indented));
                return;
            }

            foreach (var detectedPlatform in detectedPlatforms)
            {
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    ["Platform_Name"] = detectedPlatform.Platform,
                    ["Platform_Version"] = detectedPlatform.PlatformVersion ?? "Not Detected",
                };
                platformData.Add(data);
            }

            jsonPlatformData = JsonConvert.SerializeObject(platformData, Formatting.Indented);
            detectionResult["Platform_Data"] = jsonPlatformData;
            var json = JsonConvert.SerializeObject(detectionResult, Formatting.Indented);
            console.WriteLine(json);
        }
    }
}
