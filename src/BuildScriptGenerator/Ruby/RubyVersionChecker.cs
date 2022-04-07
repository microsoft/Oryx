// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    [Checker(RubyConstants.PlatformName)]
    public class RubyVersionChecker : IChecker
    {
        private readonly ILogger<RubyVersionChecker> logger;

        public RubyVersionChecker(ILogger<RubyVersionChecker> logger)
        {
            this.logger = logger;
        }

        [NotNull]
        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools)
        {
            var used = tools[RubyConstants.PlatformName];
            var comparison = SemanticVersionResolver.CompareVersions(used, RubyConstants.RubyLtsVersion);
            this.logger.LogDebug($"SemanticVersionResolver.CompareVersions returned {comparison}");
            if (comparison < 0)
            {
                return new[]
                {
                    new CheckerMessage(string.Format(
                        Resources.Labels.ToolVersionCheckerMessageFormat,
                        RubyConstants.PlatformName,
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
