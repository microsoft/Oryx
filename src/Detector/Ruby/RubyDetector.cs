// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.Detector.Ruby
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects Ruby applications.
    /// </summary>
    public class RubyDetector : IRubyPlatformDetector
    {
        private readonly ILogger<RubyDetector> logger;
        private readonly DetectorOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="RubyDetector"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{RubyDetector}"/>.</param>
        /// <param name="options">The <see cref="DetectorOptions"/>.</param>
        public RubyDetector(ILogger<RubyDetector> logger, IOptions<DetectorOptions> options)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            bool isRubyApp = false;
            string appDirectory = string.Empty;
            bool gemfileExists = false;
            var sourceRepo = context.SourceRepo;
            string bundlerVersion = string.Empty;
            bool configYmlFileExists = false;
            if (sourceRepo.FileExists(RubyConstants.ConfigYmlFileName))
            {
                configYmlFileExists = true;
                this.logger.LogInformation($"Found {RubyConstants.ConfigYmlFileName} at the root of the repo.");
            }

            if (!string.IsNullOrEmpty(this.options.AppType))
            {
                this.logger.LogInformation($"{nameof(this.options.AppType)} is set to {this.options.AppType}");

                var appType = this.options.AppType.ToLower();
                if (appType.Contains(Constants.StaticSiteApplications)
                    && configYmlFileExists)
                {
                    isRubyApp = true;
                    this.logger.LogInformation($"The ruby app was detected as a Jekyll static web app. ");
                }
            }

            if (sourceRepo.FileExists(RubyConstants.GemFileName))
            {
                isRubyApp = true;
                gemfileExists = true;
                this.logger.LogInformation($"Found {RubyConstants.GemFileName} at the root of the repo.");
            }
            else
            {
                this.logger.LogDebug(
                    $"Could not find {RubyConstants.GemFileName} in repo");
            }

            if (!isRubyApp)
            {
                var isRubyLikeApp = false;
                if (sourceRepo.FileExists(RubyConstants.GemFileLockName))
                {
                    isRubyLikeApp = true;
                    bundlerVersion = this.GetBundlerVersionFromGemFileLock(context);
                    this.logger.LogInformation($"Found {RubyConstants.GemFileLockName} "
                    + "at the root of the repo.");
                }

                if (sourceRepo.FileExists(RubyConstants.ConfigRubyFileName))
                {
                    isRubyLikeApp = true;
                    this.logger.LogInformation($"Found {RubyConstants.ConfigRubyFileName} "
                    + "at the root of the repo.");
                }

                if (isRubyLikeApp)
                {
                    foreach (var iisStartupFile in RubyConstants.IisStartupFiles)
                    {
                        if (sourceRepo.FileExists(iisStartupFile))
                        {
                            this.logger.LogDebug(
                                "App in repo is not a Ruby app as it has the file {iisStartupFile}",
                                iisStartupFile.Hash());
                            return null;
                        }
                    }

                    isRubyApp = true;
                }
                else
                {
                    this.logger.LogDebug("Could not find typical Ruby files in repo");
                }
            }

            if (!isRubyApp)
            {
                this.logger.LogDebug("App in repo is not a Ruby app");
                return null;
            }

            var version = this.GetVersion(context);
            return new RubyPlatformDetectorResult
            {
                Platform = RubyConstants.PlatformName,
                PlatformVersion = version,
                AppDirectory = appDirectory,
                GemfileExists = gemfileExists,
                BundlerVersion = bundlerVersion,
                ConfigYmlFileExists = configYmlFileExists,
            };
        }

        private string GetVersion(DetectorContext context)
        {
            var versionFromGemfile = this.GetVersionFromGemFile(context);
            if (versionFromGemfile != null)
            {
                return versionFromGemfile;
            }

            var versionFromGemfileLock = this.GetVersionFromGemFileLock(context);
            if (versionFromGemfileLock != null)
            {
                return versionFromGemfileLock;
            }

            this.logger.LogDebug("Could not get version from the gemfile or gemfile.lock. ");
            return null;
        }

        private string GetVersionFromGemFile(DetectorContext context)
        {
            try
            {
                var gemFileContent = context.SourceRepo.ReadFile(RubyConstants.GemFileName);
                var gemFileContentLines = gemFileContent.Split('\n');

                // Example content from a Gemfile:
                // source "https://rubygems.org"
                // gem 'sinatra'
                // ruby "2.5.1"
                foreach (var gemFileContentLine in gemFileContentLines)
                {
                    if (gemFileContentLine.Trim().StartsWith("ruby", StringComparison.InvariantCultureIgnoreCase))
                    {
                        var rubyVersionLine = gemFileContentLine.Trim().Split(' ');

                        // Make sure it's in valid format.
                        if (rubyVersionLine.Length == 2)
                        {
                            return rubyVersionLine[1].Trim('\"').Trim('\'');
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(
                    ex,
                    $"Exception caught while trying to parse {RubyConstants.GemFileName}");
            }

            return null;
        }

        private string GetVersionFromGemFileLock(DetectorContext context)
        {
            try
            {
                var gemFileLockContent = context.SourceRepo.ReadFile(RubyConstants.GemFileLockName);
                var gemFileLockContentLines = gemFileLockContent.Split('\n');
                gemFileLockContentLines = gemFileLockContentLines.Select(x => x.Trim()).ToArray();

                // Example content from a Gemfile.lock:
                // PLATFORMS
                //   ruby
                // RUBY VERSION
                //   ruby 2.3.1p112
                int rubyVersionLineIndex = Array.IndexOf(gemFileLockContentLines, "RUBY VERSION");
                if (rubyVersionLineIndex != -1)
                {
                    var rubyVersionLine = gemFileLockContentLines[rubyVersionLineIndex + 1].Split(' ');

                    // Make sure it's in valid format.
                    if (rubyVersionLine.Length == 2)
                    {
                        var fullVersion = rubyVersionLine[1];

                        // Parse the ruby version to remove patch versioning from it.
                        // At times, ruby will add a patch version such as p112 to the end of the semver version,
                        // which causes Oryx to not find the correct version. This should take a version like
                        // 2.3.1p112 and convert it to 2.3.1
                        var parsedVersionMatches = Regex.Match(fullVersion, @"^(.*?)(?:p[0-9]+)*$");
                        if (parsedVersionMatches.Success)
                        {
                            return parsedVersionMatches.Groups[1].Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(
                    ex,
                    $"Exception caught while trying to parse {RubyConstants.GemFileLockName}");
            }

            return null;
        }

        private string GetBundlerVersionFromGemFileLock(DetectorContext context)
        {
            try
            {
                var gemFileLockContent = context.SourceRepo.ReadFile(RubyConstants.GemFileLockName);
                var gemFileLockContentLines = gemFileLockContent.Split('\n');
                gemFileLockContentLines = gemFileLockContentLines.Select(x => x.Trim()).ToArray();

                // Example content from a Gemfile.lock:
                // BUNDLED WITH
                //   1.11.2
                int bundlerVersionLineIndex = Array.IndexOf(gemFileLockContentLines, "BUNDLED WITH");
                if (bundlerVersionLineIndex != -1)
                {
                    return gemFileLockContentLines[bundlerVersionLineIndex + 1];
                }
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(
                    ex,
                    $"Exception caught while trying to parse {RubyConstants.GemFileLockName}");
            }

            return null;
        }
    }
}
