// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Oryx.Common.Extensions;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Gets the installation script snippet which contains snippets for all detected platforms.
    /// </summary>
    public class PlatformsInstallationScriptProvider
    {
        private readonly IEnumerable<IProgrammingPlatform> _platforms;
        private readonly DefaultPlatformDetector _platformDetector;
        private readonly IStandardOutputWriter _outputWriter;

        public PlatformsInstallationScriptProvider(
            IEnumerable<IProgrammingPlatform> platforms,
            DefaultPlatformDetector platformDetector,
            IStandardOutputWriter outputWriter)
        {
            _platforms = platforms;
            _platformDetector = platformDetector;
            _outputWriter = outputWriter;
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
                detectionResults = _platformDetector.DetectPlatforms(context);
            }

            var snippets = GetInstallationScriptSnippets(detectionResults, context);
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
                var platform = _platforms
                    .Where(p => p.Name.EqualsIgnoreCase(detectionResult.Platform))
                    .First();

                var snippet = platform.GetInstallerScriptSnippet(context, detectionResult);
                if (!string.IsNullOrEmpty(snippet))
                {
                    _outputWriter.WriteLine(
                        $"Version '{detectionResult.PlatformVersion}' of platform '{detectionResult.Platform}' " +
                        $"is not installed. Generating script to install it...");
                    installationScriptSnippets.Add(snippet);
                }
            }

            return installationScriptSnippets;
        }
    }
}
