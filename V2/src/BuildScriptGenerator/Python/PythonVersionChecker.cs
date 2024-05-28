// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    [Checker(PythonConstants.PlatformName)]
    public class PythonVersionChecker : IChecker
    {
        private readonly ILogger<PythonVersionChecker> logger;

        public PythonVersionChecker(ILogger<PythonVersionChecker> logger)
        {
            this.logger = logger;
        }

        [NotNull]
        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools)
        {
            var used = tools[PythonConstants.PlatformName];
            var comparison = SemanticVersionResolver.CompareVersions(used, PythonConstants.PythonLtsVersion);
            this.logger.LogDebug($"SemanticVersionResolver.CompareVersions returned {comparison}");
            if (comparison < 0)
            {
                return new[]
                {
                    new CheckerMessage(string.Format(
                        Resources.Labels.ToolVersionCheckerMessageFormat,
                        PythonConstants.PlatformName,
                        used,
                        Constants.OryxGitHubUrl)),
                };
            }

            return Enumerable.Empty<ICheckerMessage>();
        }

        [NotNull]
        public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo)
        {
            return Enumerable.Empty<ICheckerMessage>();
        }
    }
}
