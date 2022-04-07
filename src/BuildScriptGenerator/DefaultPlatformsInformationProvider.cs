// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Detects platforms in the provided source directory.
    /// </summary>
    public class DefaultPlatformsInformationProvider
    {
        private readonly IEnumerable<IProgrammingPlatform> platforms;
        private readonly IStandardOutputWriter outputWriter;

        public DefaultPlatformsInformationProvider(
            IEnumerable<IProgrammingPlatform> platforms,
            IStandardOutputWriter outputWriter)
        {
            this.platforms = platforms;
            this.outputWriter = outputWriter;
        }

        /// <summary>
        /// Detects platforms in the provided source directory.
        /// </summary>
        /// <param name="context">The <see cref="RepositoryContext"/>.</param>
        /// <returns>A list of detected platform results.</returns>
        public IEnumerable<PlatformInfo> GetPlatformsInfo(RepositoryContext context)
        {
            var platformInfos = new List<PlatformInfo>();

            // Try detecting ALL platforms since in some scenarios this is required.
            // For example, in case of a multi-platform app like ASP.NET Core + NodeJs, we might need to dynamically
            // install both these platforms' sdks before actually using any of their commands. So even though a user
            // of Oryx might explicitly supply the platform of the app as .NET Core, we still need to make sure the
            // build environment is setup with detected platforms' sdks.
            this.outputWriter.WriteLine("Detecting platforms...");

            foreach (var platform in this.platforms)
            {
                // Check if a platform is enabled or not
                if (!platform.IsEnabled(context))
                {
                    this.outputWriter.WriteLine(
                        $"Platform '{platform.Name}' has been disabled, so skipping detection for it.");
                    continue;
                }

                var detectionResult = platform.Detect(context);

                if (detectionResult != null)
                {
                    var toolsInPath = platform.GetToolsToBeSetInPath(context, detectionResult);

                    platformInfos.Add(new PlatformInfo
                    {
                        DetectorResult = detectionResult,
                        RequiredToolsInPath = toolsInPath,
                    });
                }
            }

            if (platformInfos.Any())
            {
                this.outputWriter.WriteLine("Detected following platforms:");
                foreach (var platformInfo in platformInfos)
                {
                    var detectorResult = platformInfo.DetectorResult;
                    this.outputWriter.WriteLine($"  {detectorResult.Platform}: {detectorResult.PlatformVersion}");
                }
            }
            else
            {
                this.outputWriter.WriteLine("Could not detect any platform in the source directory.");
            }

            return platformInfos;
        }
    }
}
