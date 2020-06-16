// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Hugo
{
    internal class HugoPlatformDetector : IPlatformDetector
    {
        private readonly IEnvironment _environment;

        public HugoPlatformDetector(IEnvironment environment)
        {
            _environment = environment;
        }

        public virtual PlatformDetectorResult Detect(RepositoryContext context)
        {
            var isHugoApp = StaticSiteGeneratorHelper.IsHugoApp(context.SourceRepo, _environment);
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
