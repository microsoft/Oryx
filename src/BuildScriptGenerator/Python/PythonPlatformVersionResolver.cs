// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonPlatformVersionResolver : IPlatformVersionResolver
    {
        private readonly ILogger<PythonPlatformVersionResolver> _logger;
        private readonly IPythonVersionProvider _versionProvider;

        public PythonPlatformVersionResolver(
           ILogger<PythonPlatformVersionResolver> logger,
           IPythonVersionProvider versionProvider)
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
                    PythonConstants.PlatformName,
                    version,
                    versionInfo.SupportedVersions);
                _logger.LogError(
                    exc,
                    $"Exception caught, the version '{version}' is not supported for the Python platform.");
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
