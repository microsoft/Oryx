// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Tests;
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

        [Option("--json", Description = "Output the detected platform data in JSON format.")]
        public bool OutputJson { get; set; }

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
                if (detectedPlatforms != null && detectedPlatforms.Any())
                {
                    if (OutputJson)
                    {
                        console.WriteLine("Detection result in Json format:");
                        console.WriteLine(JsonFormatResult(detectedPlatforms));
                    }
                    else
                    {
                        console.WriteLine("Detection result:");
                        console.WriteLine(ListFormatResult(detectedPlatforms));
                    }

                    return ProcessConstants.ExitSuccess;
                }
            }

            return ProcessConstants.ExitFailure;
        }

        internal override bool IsValidInput(IServiceProvider serviceProvider, IConsole console)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<DetectCommand>>();
            var options = serviceProvider.GetRequiredService<IOptions<DetectorOptions>>().Value;
            SourceDir = string.IsNullOrEmpty(SourceDir) ? Directory.GetCurrentDirectory() : Path.GetFullPath(SourceDir);

            if (!Directory.Exists(SourceDir))
            {
                logger?.LogError("Could not find the source directory.");
                console.WriteErrorLine($"Could not find the source directory: '{SourceDir}'");

                return false;
            }

            return true;
        }

        private string ListFormatResult(IDictionary<PlatformName, string> detectedPlatforms)
        {
            var result = new StringBuilder();

            foreach (var detectedPlatform in detectedPlatforms)
            {
                var defs = new DefinitionListFormatter();

                defs.AddDefinition("Platform", detectedPlatform.Key.ToString());
                defs.AddDefinition("Version", detectedPlatform.Value != null ? detectedPlatform.Value : "N/A");

                result.AppendLine(defs.ToString());
            }

            return result.ToString();
        }

        private string JsonFormatResult(IDictionary<PlatformName, string> detectedPlatforms)
        {
            var detectionResult = new Dictionary<string, string>();
            detectionResult["App_Path"] = SourceDir;
            var platformData = new List<Dictionary<string, string>>();
            foreach (var detectedPlatform in detectedPlatforms)
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data["Platform_Name"] = detectedPlatform.Key.ToString();
                data["Platform_Version"] = detectedPlatform.Value;
                platformData.Add(data);
            }

            var jsonPlatformData = JsonConvert.SerializeObject(platformData, Formatting.Indented);
            detectionResult["Platform_Data"] = jsonPlatformData;
            var json = JsonConvert.SerializeObject(detectionResult, Formatting.Indented);
            return json;
        }
    }
}
