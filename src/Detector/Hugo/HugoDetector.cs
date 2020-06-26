// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Oryx.Detector.Hugo
{
    public class HugoDetector : IPlatformDetector
    {
        private readonly HugoDetectorOptions _options;
        private readonly ILogger<HugoDetector> _logger;

        public HugoDetector(
            IOptions<HugoDetectorOptions> options,
            ILogger<HugoDetector> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public PlatformName DetectorPlatformName => PlatformName.Hugo;

        public PlatformDetectorResult Detect(DetectorContext context)
        {
            var isHugoApp = StaticSiteGeneratorHelper.IsHugoApp(context.SourceRepo, _options);
            if (isHugoApp)
            {
                return new PlatformDetectorResult
                {
                    Platform = HugoConstants.PlatformName,
                    PlatformVersion = HugoConstants.Version,
                };
            }

            return null;
        }
    }
}
