// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.Detector.Php
{
    public class PhpDetector : IPlatformDetector
    {
        private readonly ILogger<PhpDetector> _logger;

        public PhpDetector(
            ILogger<PhpDetector> logger)
        {
            _logger = logger;
        }

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            var sourceRepo = context.SourceRepo;
            if (!sourceRepo.FileExists(PhpConstants.ComposerFileName))
            {
                _logger.LogDebug($"File '{PhpConstants.ComposerFileName}' does not exist in source repo");
                return null;
            }

            var version = GetVersion(context);

            return new PlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = version,
            };
        }

        public PlatformName GetDetectorPlatformName => PlatformName.Php;

        private string GetVersion(RepositoryContext context)
        {
            if (context.ResolvedPhpVersion != null)
            {
                return context.ResolvedPhpVersion;
            }

            var version = GetVersionFromComposerFile(context);
            if (version != null)
            {
                return version;
            }
            _logger.LogDebug("Could not get version from the composer file. Getting default version.");
            return GetDefaultVersionFromProvider();
        }

        private string GetVersionFromComposerFile(RepositoryContext context)
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

        private string GetDefaultVersionFromProvider()
        {
            return PhpConstants.PhpDefaultVersion;
        }

    }
}
