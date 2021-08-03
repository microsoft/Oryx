// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.RegularExpressions;

namespace Microsoft.Oryx.Detector.Go
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects Go applications.
    /// </summary>
    class GoDetector : IGoPlatformDetector
    {
        private readonly ILogger<GoDetector> _logger;
        private readonly DetectorOptions _options;

        /// <summary>
        /// Creates an instance of <see cref="GoDetector"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{GoDetector}"/>.</param>
        /// <param name="options">The <see cref="DetectorOptions"/>.</param>
        public GoDetector(ILogger<GoDetector> logger, IOptions<DetectorOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            string appDirectory = string.Empty;
            var sourceRepo = context.SourceRepo;
            // check if go.mod exists
            if (!sourceRepo.FileExists(GoConstants.GoDotModFileName))
            {
                _logger.LogDebug(
                    $"Could not find {GoConstants.GoDotModFileName} in repo");
                return null;
            }
            _logger.LogInformation($"Found {GoConstants.GoDotModFileName} at the root of the repo. ");

            // TODO: check if go.sum or /cmd/repo_name/*.go 
            var version = GetVersion(context);

            // TODO: add additional fields that are helpful
            return new GoPlatformDetectorResult
            {
                Platform = GoConstants.PlatformName,
                PlatformVersion = version,
                AppDirectory = appDirectory,
            };
        }

        private string GetVersion(DetectorContext context)
        {
            var versionFromGoDotMod = GetVersionFromGoDotMod(context);
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
                var goDotModFileContent = context.SourceRepo.ReadFile(GoConstants.GoDotModFileName);
                var goDotModFileContentLines = goDotModFileContent.Split('\n');
                var sourceRepo = context.SourceRepo;
                // Example content of go.mod:
                // module myModule
                //
                // go 1.16
                Regex regex = new Regex(@"^[\s]*go [0-9]+\.[0-9]+");
                foreach (var goDotModFileContentLine in goDotModFileContentLines)
                {
                    var goVersionLine = goDotModFileContentLine.Trim().Split(' ');
                    Match match = regex.Match(goDotModFileContentLine);
                    if (match.Success)
                    {
                        return goVersionLine[1].Trim('\"').Trim('\'');
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    $"Exception caught while trying to parse {GoConstants.GoDotModFileName}");
            }

            return null;
        }
    }
}
