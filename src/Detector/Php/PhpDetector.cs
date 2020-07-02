// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Oryx.Common.Extensions;
using Newtonsoft.Json;

namespace Microsoft.Oryx.Detector.Php
{
    public class PhpDetector : IPhpPlatformDetector
    {
        private readonly ILogger<PhpDetector> _logger;

        public PhpDetector(
            ILogger<PhpDetector> logger)
        {
            _logger = logger;
        }

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            var sourceRepo = context.SourceRepo;
            var composerFileExists = sourceRepo.FileExists(PhpConstants.ComposerFileName);
            var composerLockFileExists = sourceRepo.FileExists(PhpConstants.ComposerLockFileName);

            if (!composerFileExists && !composerLockFileExists)
            {
                _logger.LogDebug(
                    $"Files '{PhpConstants.ComposerFileName}' or '{PhpConstants.ComposerLockFileName}' " +
                    $"do not exist in source repo.");
                return null;
            }

            var version = GetVersion(context);

            return new PhpPlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = version,
                ComposerFileExists = composerFileExists,
                ComposerLockFileExists = composerLockFileExists,
            };
        }

        private string GetVersion(DetectorContext context)
        {

            var version = GetVersionFromComposerFile(context);
            if (version != null)
            {
                return version;
            }
            _logger.LogDebug("Could not get version from the composer file. ");
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
                _logger.LogWarning(
                    ex,
                    $"Exception caught while trying to deserialize {PhpConstants.ComposerFileName.Hash()}");
            }

            return composerFile?.require?.php?.Value as string;
        }
    }
}
