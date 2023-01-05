// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Oryx.Common.Extensions;
using Microsoft.Oryx.Detector;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Gets the installation script snippet which contains snippets for all detected platforms.
    /// </summary>
    public class PlatformsInstallationScriptProvider
    {
        private readonly IEnumerable<IProgrammingPlatform> platforms;
        private readonly DefaultPlatformsInformationProvider platformDetector;
        private readonly IStandardOutputWriter outputWriter;

        public PlatformsInstallationScriptProvider(
            IEnumerable<IProgrammingPlatform> platforms,
            DefaultPlatformsInformationProvider platformDetector,
            IStandardOutputWriter outputWriter)
        {
            this.platforms = platforms;
            this.platformDetector = platformDetector;
            this.outputWriter = outputWriter;
        }

        /// <summary>
        /// Gets the installation script snippet which contains snippets for all detected platforms.
        /// </summary>
        /// <param name="context">The <see cref="RepositoryContext"/>.</param>
        /// <param name="detectionResults">Already detected platforms results.</param>
        /// <returns>A snippet having logic to install all detected platforms.</returns>
        public string GetBashScriptSnippet(
            BuildScriptGeneratorContext context,
            IEnumerable<PlatformDetectorResult> detectionResults = null)
        {
            var scriptBuilder = new StringBuilder();

            // Avoid detecting again if detection was already run.
            if (detectionResults == null)
            {
                var platformInfos = this.platformDetector.GetPlatformsInfo(context);
                if (platformInfos != null)
                {
                    detectionResults = platformInfos.Select(pi => pi.DetectorResult);
                }
            }

            var snippets = this.GetInstallationScriptSnippets(detectionResults, context);
            foreach (var snippet in snippets)
            {
                scriptBuilder.AppendLine(snippet);
                scriptBuilder.AppendLine();
            }

            return scriptBuilder.ToString();
        }

        private IEnumerable<string> GetInstallationScriptSnippets(
            IEnumerable<PlatformDetectorResult> detectionResults,
            BuildScriptGeneratorContext context)
        {
            var installationScriptSnippets = new List<string>();

            foreach (var detectionResult in detectionResults)
            {
                var platform = this.platforms
                    .Where(p => p.Name.EqualsIgnoreCase(detectionResult.Platform))
                    .First();

                var snippet = platform.GetInstallerScriptSnippet(context, detectionResult);
                if (!string.IsNullOrEmpty(snippet))
                {
                    this.outputWriter.WriteLine(
                        $"Version '{detectionResult.PlatformVersion}' of platform '{detectionResult.Platform}' " +
                        $"is not installed. Generating script to install it...");
                    installationScriptSnippets.Add(snippet);
                }
            }

            return installationScriptSnippets;
        }
    }
}
