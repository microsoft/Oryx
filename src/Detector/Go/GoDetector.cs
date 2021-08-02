using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common.Extensions;
using System;
using System.Linq;

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
            bool isGoApp = false;
            string appDirectory = string.Empty;
            var sourceRepo = context.SourceRepo;
            bool goDotModExists = false;
            // check if go.mod exists
            if (sourceRepo.FileExists(GoConstants.GoDotModFileName))
            {
                goDotModExists = true;
                isGoApp = true;
                _logger.LogInformation($"Found {GoConstants.GoDotModFileName} at the root of the repo. ");
            }
            else
            {
                _logger.LogDebug(
                    $"Could not find {GoConstants.GoDotModFileName} in repo");
                return null;
            }

            // TODO: check if go.sum or /cmd/repo_name/*.go exists

            var version = GetVersion(context);
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

                // Example content of go.mod:
                // module myModule
                //
                // go 1.16
                foreach (var goDotModFileContentLine in goDotModFileContentLines)
                {
                    if (goDotModFileContentLine.Trim().StartsWith("go", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var goVersionLine = goDotModFileContentLine.Trim().Split(' ');
                        // Make sure it's in valid format
                        if (goVersionLine.Length == 2)
                        {
                            return goVersionLine[1].Trim('\"').Trim('\'');
                        }
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
