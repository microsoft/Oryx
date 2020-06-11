// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Php;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    internal class PhpPlatformDetector : IPlatformDetector
    {
        private readonly PhpScriptGeneratorOptions _options;
        private readonly ILogger<PhpPlatformDetector> _logger;
        private readonly IStandardOutputWriter _writer;
        private readonly IPlatformDetector _detector;
        private readonly IPlatformVersionResolver _versionResolver;

        public PlatformName GetDetectorPlatformName => PlatformName.Php;

        public PhpPlatformDetector(
            IOptions<PhpScriptGeneratorOptions> options,
            ILogger<PhpPlatformDetector> logger,
            IStandardOutputWriter writer,
            PhpDetector detector,
            PhpPlatformVersionResolver versionResolver)
        {
            _options = options.Value;
            _logger = logger;
            _writer = writer;
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

            var version = _versionResolver.GetMaxSatisfyingVersionAndVerify(platformDetectorResult.PlatformVersion);

            return new PlatformDetectorResult
            {
                Platform = PhpConstants.PlatformName,
                PlatformVersion = version,
            };
        }

        public string GetMaxSatisfyingVersionAndVerify(string version)
        {
            return _versionResolver.GetMaxSatisfyingVersionAndVerify(version);
        }
    }
}
