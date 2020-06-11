// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpPlatformVersionResolver : IPlatformVersionResolver
    {
        private readonly ILogger<PhpPlatformVersionResolver> _logger;
        private readonly IPhpVersionProvider _versionProvider;

        public PhpPlatformVersionResolver(
           ILogger<PhpPlatformVersionResolver> logger,
           IPhpVersionProvider versionProvider)
        {
            _logger = logger;
            _versionProvider = versionProvider;
        }


        public string GetMaxSatisfyingVersionAndVerify(string version)
        {
            var versionInfo = _versionProvider.GetVersionInfo();
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                version,
                versionInfo.SupportedVersions);

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exc = new UnsupportedVersionException(
                    PhpConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                _logger.LogError(
                    exc,
                    $"Exception caught, the version '{version}' is not supported for the PHP platform.");
                throw exc;
            }

            return maxSatisfyingVersion;
        }

        public string GetDefaultVersionFromProvider()
        {
            var versionInfo = _versionProvider.GetVersionInfo();
            return versionInfo.DefaultVersion;
        }
    }
}
