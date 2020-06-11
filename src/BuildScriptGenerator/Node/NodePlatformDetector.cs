// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.Common;
using Microsoft.Oryx.Detector;
using Microsoft.Oryx.Detector.Node;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    internal class NodePlatformDetector : IBuildScriptGenerationDetector
    {
        private readonly NodeScriptGeneratorOptions _options;
        private readonly ILogger<NodePlatformDetector> _logger;
        private readonly IEnvironment _environment;
        private readonly IStandardOutputWriter _writer;
        private readonly IPlatformDetector _detector;
        private readonly IPlatformVersionResolver _versionResolver;

        public PlatformName DetectorPlatformName => PlatformName.Node;

        public NodePlatformDetector(
            IOptions<NodeScriptGeneratorOptions> options,
            ILogger<NodePlatformDetector> logger,
            IEnvironment environment,
            IStandardOutputWriter writer,
            NodeDetector detector,
            NodePlatformVersionResolver versionResolver
            )
        {
            _options = options.Value;
            _logger = logger;
            _environment = environment;
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
                Platform = NodeConstants.PlatformName,
                PlatformVersion = version,
            };
        }

        public string GetMaxSatisfyingVersionAndVerify(string version)
        {
            return _versionResolver.GetMaxSatisfyingVersionAndVerify(version);
        }
    }
}