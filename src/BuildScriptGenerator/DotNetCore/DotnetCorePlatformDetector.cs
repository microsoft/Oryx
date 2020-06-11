// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.DotNetCore;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    internal class DotNetCorePlatformDetector : IBuildScriptGenerationDetector
    {
        private readonly DotNetCoreScriptGeneratorOptions _options;
        private readonly ILogger<DotNetCorePlatformDetector> _logger;
        private readonly IPlatformDetector _detector;
        private readonly IPlatformVersionResolver _versionResolver;

        public PlatformName DetectorPlatformName => PlatformName.DotNetCore;

        public DotNetCorePlatformDetector(
            IOptions<DotNetCoreScriptGeneratorOptions> options,
            ILogger<DotNetCorePlatformDetector> logger,
            DotNetCoreDetector detector,
            DotNetCorePlatformVersionResolver versionResolver)
        {
            _options = options.Value;
            _logger = logger;
            _detector = detector;
            _versionResolver = versionResolver;
        }

        public PlatformDetectorResult Detect(RepositoryContext context)
        {
            PlatformDetectorResult platformDetectorResult = _detector.Detect(context);

            if (platformDetectorResult == null)
            {
                return null;
            }

            if (platformDetectorResult.PlatformVersion == null)
            {
                platformDetectorResult.PlatformVersion = _versionResolver.GetDefaultVersionFromProvider();
            }

            string version = _versionResolver.GetMaxSatisfyingVersionAndVerify(platformDetectorResult.PlatformVersion);

            return new PlatformDetectorResult
            {
                Platform = DotNetCoreConstants.PlatformName,
                PlatformVersion = version,
            };
        }

        public string GetMaxSatisfyingVersionAndVerify(string runtimeVersion)
        {
            return _versionResolver.GetMaxSatisfyingVersionAndVerify(runtimeVersion);
        }
    }
}