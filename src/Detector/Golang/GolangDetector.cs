// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.RegularExpressions;

namespace Microsoft.Oryx.Detector.Golang
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects Go applications.
    /// </summary>
    class GolangDetector : IGolangPlatformDetector
    {
        private readonly ILogger<GolangDetector> _logger;
        private readonly DetectorOptions _options;

        /// <summary>
        /// Creates an instance of <see cref="GolangDetector"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{GolangDetector}"/>.</param>
        /// <param name="options">The <see cref="DetectorOptions"/>.</param>
        public GolangDetector(ILogger<GolangDetector> logger, IOptions<DetectorOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            string appDirectory = string.Empty;
            var sourceRepo = context.SourceRepo;
            // check if go.mod exists
            if (!sourceRepo.FileExists(GolangConstants.GoModFileName))
            {
                _logger.LogError(
                    $"Could not find {GolangConstants.GoModFileName} in repo, please add {GolangConstants.GoModFileName}");
                return null;
            }
            _logger.LogInformation($"Found {GolangConstants.GoModFileName} at the root of the repo. ");

            var version = GetVersion(context);

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
                var goDotModFileContent = context.SourceRepo.ReadFile(GolangConstants.GoModFileName);
                var goDotModFileContentLines = goDotModFileContent.Split('\n');
                var sourceRepo = context.SourceRepo;
                // Example content of go.mod:
                // module myModule
                //
                // go 1.16

                // Regex matching valid version format:
                //      go 1.16
                //      go 1.16.7
                Regex regex = new Regex(@"^[\s]*go[\s]+[0-9]+(\.([0-9])+)+[\s]*$");
                foreach (var goDotModFileContentLine in goDotModFileContentLines)
                {
                    Match match = regex.Match(goDotModFileContentLine);
                    if (match.Success)
                    {
                        return goDotModFileContentLine.Trim().Split(' ')[1].Trim('\"').Trim('\'');
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    $"Exception caught while trying to parse {GolangConstants.GoModFileName}." );
            }

            return null;
        }
    }
}
