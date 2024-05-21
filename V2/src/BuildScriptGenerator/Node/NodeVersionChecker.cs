// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    [Checker(NodeConstants.NodeToolName)]
    public class NodeVersionChecker : IChecker
    {
        private readonly ILogger<NodeVersionChecker> logger;
        private readonly BuildScriptGeneratorOptions options;

        public NodeVersionChecker(
            IOptions<BuildScriptGeneratorOptions> options,
            ILogger<NodeVersionChecker> logger)
        {
            this.logger = logger;
            this.options = options.Value;
        }

        [NotNull]
        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools)
        {
            var used = tools[NodeConstants.NodeToolName];
            var comparison = SemanticVersionResolver.CompareVersions(
                used,
                this.options.DebianFlavor != OsTypes.DebianStretch
                    ? NodeConstants.NodeLtsVersion
                    : FinalStretchVersions.FinalStretchNode14Version);
            this.logger.LogDebug($"SemanticVersionResolver.CompareVersions returned {comparison}");
            if (comparison < 0)
            {
                return new[]
                {
                    new CheckerMessage(string.Format(
                        Resources.Labels.ToolVersionCheckerMessageFormat,
                        NodeConstants.NodeToolName,
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
