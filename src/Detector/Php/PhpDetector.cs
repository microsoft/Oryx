// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Common.Extensions;
using Newtonsoft.Json;

namespace Microsoft.Oryx.Detector.Php
{
    /// <summary>
    /// An implementation of <see cref="IPlatformDetector"/> which detects PHP applications.
    /// </summary>
    public class PhpDetector : IPhpPlatformDetector
    {
        private readonly ILogger<PhpDetector> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhpDetector"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{PhpDetector}"/>.</param>
        public PhpDetector(ILogger<PhpDetector> logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public PlatformDetectorResult Detect(DetectorContext context)
        {
            var sourceRepo = context.SourceRepo;

            string phpVersion = null;
            var hasComposerFile = sourceRepo.FileExists(PhpConstants.ComposerFileName);
            var appDirectory = string.Empty;
            if (hasComposerFile)
            {
                this.logger.LogDebug($"File '{PhpConstants.ComposerFileName}' exists in source repo");
                phpVersion = this.GetVersion(context);
            }
            else
            {
                this.logger.LogDebug($"File '{PhpConstants.ComposerFileName}' does not exist in source repo");

                var files = sourceRepo.EnumerateFiles(PhpConstants.PhpFileNamePattern, searchSubDirectories: true);
                if (files != null && files.Any())
                {
                    this.logger.LogInformation(
                        $"Found files with extension '{PhpConstants.PhpFileNamePattern}' " +
                        $"in the repo.");
                    appDirectory = RelativeDirectoryHelper.GetRelativeDirectoryToRoot(files.FirstOrDefault(), sourceRepo.RootPath);
                }
                else
                {
                    this.logger.LogInformation(
                        $"Could not find any file with extension '{PhpConstants.PhpFileNamePattern}' " +
                        $"in the repo.");
                    return null;
                }
            }

            return new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = phpVersion,
                AppDirectory = appDirectory,
            };
        }

        private string GetVersion(DetectorContext context)
        {
            var version = this.GetVersionFromComposerFile(context);
            if (version != null)
            {
                return version;
            }

            this.logger.LogDebug("Could not get version from the composer file. ");
            return null;
        }

        private string GetVersionFromComposerFile(DetectorContext context)
        {
            dynamic composerFile = null;
            try
            {
                var jsonContent = context.SourceRepo.ReadFile(PhpConstants.ComposerFileName);
                composerFile = JsonConvert.DeserializeObject(jsonContent);
            }
            catch (Exception ex)
            {
                // We just ignore errors, so we leave malformed composer.json files for Composer to handle,
                // not us. This prevents us from erroring out when Composer itself might be able to tolerate
                // some errors in the composer.json file.
                this.logger.LogWarning(
                    ex,
                    $"Exception caught while trying to deserialize {PhpConstants.ComposerFileName.Hash()}");
            }

            return composerFile?.require?.php?.Value as string;
        }
    }
}
