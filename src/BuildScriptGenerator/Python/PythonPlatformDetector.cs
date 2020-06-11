// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Exceptions;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Python;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    internal class PythonPlatformDetector : IPlatformDetector
    {
        private readonly PythonScriptGeneratorOptions _options;
        private readonly ILogger<PythonPlatformDetector> _logger;
        private readonly IPlatformDetector _detector;
        private readonly IPlatformVersionResolver _versionResolver;

        public PlatformName GetDetectorPlatformName => PlatformName.Python;

        public PythonPlatformDetector(
            IOptions<PythonScriptGeneratorOptions> options,
            ILogger<PythonPlatformDetector> logger,
            PythonDetector detector,
            PythonPlatformVersionResolver versionResolver)
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

            var version = _versionResolver.GetMaxSatisfyingVersionAndVerify(platformDetectorResult.PlatformVersion);

            return new PlatformDetectorResult
            {
                Platform = PythonConstants.PlatformName,
                PlatformVersion = version,
            };
        }

        public string GetMaxSatisfyingVersionAndVerify(string version)
        {
            return _versionResolver.GetMaxSatisfyingVersionAndVerify(version);
        }

    }
}