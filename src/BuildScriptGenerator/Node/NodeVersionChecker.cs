// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    [Checker(NodeConstants.NodeToolName)]
    public class NodeVersionChecker : IChecker
    {
        private readonly ILogger<NodeVersionChecker> _logger;

        public NodeVersionChecker(ILogger<NodeVersionChecker> logger)
        {
            _logger = logger;
        }

        [NotNull]
        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools)
        {
            var used = tools[NodeConstants.NodeToolName];
            var comparison = SemanticVersionResolver.CompareVersions(used, NodeScriptGeneratorOptionsSetup.NodeLtsVersion);
            _logger.LogDebug($"SemanticVersionResolver.CompareVersions returned {comparison}");
            if (comparison < 0)
            {
                return new[]
                {
                    new CheckerMessage($"An outdated version of Node.js was detected ({used}). Consider updating.\n" +
                                       $"Versions supported by Oryx: {Constants.OryxGitHubUrl}")
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
