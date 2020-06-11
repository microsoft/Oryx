// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCorePlatformVersionResolver : IPlatformVersionResolver
    {
        private readonly IDotNetCoreVersionProvider _versionProvider;
        private readonly ILogger<DotNetCorePlatformVersionResolver> _logger;

        public DotNetCorePlatformVersionResolver(
           IDotNetCoreVersionProvider versionProvider,
           ILogger<DotNetCorePlatformVersionResolver> logger)
        {
            _versionProvider = versionProvider;
            _logger = logger;
        }


        public string GetMaxSatisfyingVersionAndVerify(string runtimeVersion)
        {
            var versionMap = _versionProvider.GetSupportedVersions();

            // Since our semantic versioning library does not work with .NET Core preview version format, here
            // we do some trivial way of finding the latest version which matches a given runtime version
            // Runtime versions are usually like: 1.0, 2.1, 3.1, 5.0 etc.
            // (these are constructed from netcoreapp21, netcoreapp31 etc.)
            // Preview version of sdks also have preview versions of runtime versions and hence they
            // have '-' in their names.
            var nonPreviewRuntimeVersions = versionMap.Keys.Where(version => version.IndexOf("-") < 0);
            var maxSatisfyingVersion = SemanticVersionResolver.GetMaxSatisfyingVersion(
                runtimeVersion,
                nonPreviewRuntimeVersions);

            // Check if a preview version is available
            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                // NOTE:
                // Preview versions: 5.0.0-preview.3.20214.6, 5.0.0-preview.2.20160.6, 5.0.0-preview.1.20120.5
                var previewRuntimeVersions = versionMap.Keys
                    .Where(version => version.IndexOf("-") >= 0)
                    .Where(version => version.StartsWith(runtimeVersion))
                    .OrderByDescending(version => version);
                if (previewRuntimeVersions.Any())
                {
                    maxSatisfyingVersion = previewRuntimeVersions.First();
                }
            }

            if (string.IsNullOrEmpty(maxSatisfyingVersion))
            {
                var exception = new UnsupportedVersionException(
                    DotNetCoreConstants.PlatformName,
                    runtimeVersion,
                    versionMap.Keys);
                _logger.LogError(
                    exception,
                    $"Exception caught, the version '{runtimeVersion}' is not supported for the .NET Core platform.");
                throw exception;
            }

            return maxSatisfyingVersion;
        }

        public string GetDefaultVersionFromProvider()
        {
            return _versionProvider.GetDefaultRuntimeVersion();
        }
    }
}
