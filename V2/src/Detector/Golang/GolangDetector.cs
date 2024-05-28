// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.Golang
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects Go applications.
    /// </summary>
    public class GolangDetector : IGolangPlatformDetector
    {
        private readonly ILogger<GolangDetector> logger;
        private readonly DetectorOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="GolangDetector"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{GolangDetector}"/>.</param>
        /// <param name="options">The <see cref="DetectorOptions"/>.</param>
        public GolangDetector(ILogger<GolangDetector> logger, IOptions<DetectorOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            string appDirectory = string.Empty;
            var sourceRepo = context.SourceRepo;

            // check if go.mod exists
            if (!sourceRepo.FileExists(GolangConstants.GoModFileName))
            {
                this.logger.LogError(
                    $"Could not find {GolangConstants.GoModFileName} in repo");
                return null;
            }

            this.logger.LogInformation($"Found {GolangConstants.GoModFileName} at the root of the repo. ");

            var version = this.GetVersion(context);

            // TODO: add additional fields that are helpful
            return new GolangPlatformDetectorResult
            {
                Platform = GolangConstants.PlatformName,
                GoModExists = true,
                PlatformVersion = version,
                AppDirectory = appDirectory,
            };
        }

        private string GetVersion(DetectorContext context)
        {
            var versionFromGoDotMod = this.GetVersionFromGoDotMod(context);
            if (versionFromGoDotMod != null)
            {
                return versionFromGoDotMod;
            }

            return null;
        }

        private string GetVersionFromGoDotMod(DetectorContext context)
        {
            try
            {
                var goDotModFileContent = context.SourceRepo.ReadFile(GolangConstants.GoModFileName);
                var goDotModFileContentLines = goDotModFileContent.Split('\n');
                var sourceRepo = context.SourceRepo;

                // Example content of go.mod:
                // module myModule
                //
                // go 1.16

                // Match regex:
                //     - start with 0 or more white spaces
                //     - match string: go
                //     - 0 or more white spaces
                //     - digit(s)
                //     - (a period followed by digit(s)) once or twice
                //     - any number of white spaces
                // Regex matching valid version format:
                //      go 1.16
                //      go 1.16.7
                Regex regex = new Regex(@"^[\s]*go[\s]+[0-9]+(\.([0-9])+){1,2}[\s]*$");
                foreach (var goDotModFileContentLine in goDotModFileContentLines)
                {
                    Match match = regex.Match(goDotModFileContentLine);
                    if (match.Success)
                    {
                        // After matching regex is found we trim off 'go' and trailing quotes
                        // allowing us to only retain the version.
                        // Example: "go 1.16.7" -> 1.16.7
                        return goDotModFileContentLine.Trim().Split(' ')[1].Trim('\"').Trim('\'');
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    $"Exception caught while trying to parse {GolangConstants.GoModFileName}.");
            }

            return null;
        }
    }
}
