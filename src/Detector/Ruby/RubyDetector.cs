// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common.Extensions;
using System;
using System.Linq;

namespace Microsoft.Oryx.Detector.Ruby
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects Ruby applications.
    /// </summary>
    public class RubyDetector : IRubyPlatformDetector
    {
        private readonly ILogger<RubyDetector> _logger;
        private readonly DetectorOptions _options;

        /// <summary>
        /// Creates an instance of <see cref="RubyDetector"/>.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{RubyDetector}"/>.</param>
        /// <param name="options">The <see cref="DetectorOptions"/>.</param>
        public RubyDetector(ILogger<RubyDetector> logger, IOptions<DetectorOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            bool isRubyApp = false;
            string appDirectory = string.Empty;
            bool gemfileExists = false;
            var sourceRepo = context.SourceRepo;
            string bundlerVersion = string.Empty;
            bool configYmlFileExists = false;
            if (!string.IsNullOrEmpty(_options.AppType))
            {
                _logger.LogInformation($"{nameof(_options.AppType)} is set to {_options.AppType}");

                var appType = _options.AppType.ToLower();
                if (appType.Contains(Constants.StaticSiteApplications)
                    && sourceRepo.FileExists(RubyConstants.ConfigYmlFileName))
                {
                    isRubyApp = true;
                    configYmlFileExists = true;
                }
            }
            if (sourceRepo.FileExists(RubyConstants.GemFileName))
            {
                isRubyApp = true;
                gemfileExists = true;
                _logger.LogInformation($"Found {RubyConstants.GemFileName} at the root of the repo.");
            }
            else
            {
                _logger.LogDebug(
                    $"Could not find {RubyConstants.GemFileName} in repo");
            }
            if (!isRubyApp) {
                var isRubyLikeApp = false;
                if (sourceRepo.FileExists(RubyConstants.GemFileLockName))
                {
                    isRubyLikeApp = true;
                    bundlerVersion = GetBundlerVersionFromGemFileLock(context);
                    _logger.LogInformation($"Found {RubyConstants.GemFileLockName} "
                    + "at the root of the repo.");
                }
                if (sourceRepo.FileExists(RubyConstants.ConfigRubyFileName))
                {
                    isRubyLikeApp = true;
                    _logger.LogInformation($"Found {RubyConstants.ConfigRubyFileName} "
                    + "at the root of the repo.");
                }
                if (isRubyLikeApp) {
                     foreach (var iisStartupFile in RubyConstants.IisStartupFiles)
                    {
                        if (sourceRepo.FileExists(iisStartupFile))
                        {
                            _logger.LogDebug(
                                "App in repo is not a Ruby app as it has the file {iisStartupFile}",
                                iisStartupFile.Hash());
                            return null;
                        }
                    }
                    isRubyApp = true;
                }
                else
                {
                    _logger.LogDebug("Could not find typical Ruby files in repo");
                }
            }
            if (!isRubyApp)
            {
                _logger.LogDebug("App in repo is not a Ruby app");
                return null;
            }
            var version = GetVersion(context);
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
            var versionFromGemfile = GetVersionFromGemFile(context);
            if (versionFromGemfile != null)
            {
                return versionFromGemfile;
            }
            var versionFromGemfileLock = GetVersionFromGemFileLock(context);
            if (versionFromGemfileLock != null) {
                return versionFromGemfileLock;
            }
            _logger.LogDebug("Could not get version from the gemfile or gemfile.lock. ");
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
                _logger.LogWarning(
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
                        return rubyVersionLine[1];
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    $"Exception caught while trying to parse {RubyConstants.GemFileLockName}");
            }
            return null;
        }

        private string GetBundlerVersionFromGemFileLock(DetectorContext context) {
            try
            {
                var gemFileLockContent = context.SourceRepo.ReadFile(RubyConstants.GemFileLockName);
                var gemFileLockContentLines = gemFileLockContent.Split('\n');
                gemFileLockContentLines = gemFileLockContentLines.Select(x => x.Trim()).ToArray();
                // Example content from a Gemfile.lock:
                //BUNDLED WITH
                //   1.11.2
                int bundlerVersionLineIndex = Array.IndexOf(gemFileLockContentLines, "BUNDLED WITH");
                if (bundlerVersionLineIndex != -1)
                {
                    return gemFileLockContentLines[bundlerVersionLineIndex + 1];
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    $"Exception caught while trying to parse {RubyConstants.GemFileLockName}");
            }
            return null;
        }
    }
}