// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    [Checker(NodeConstants.NodeToolName)]
    public class NodeVersionChecker : IChecker
    {
        [NotNull]
        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools)
        {
            if (tools.ContainsKey(NodeConstants.NodeJsName))
            {
                var used = tools[NodeConstants.NodeJsName];
                if (SemanticVersionResolver.CompareVersions(used, NodeScriptGeneratorOptionsSetup.NodeLtsVersion) < 0)
                {
                    return new[]
                    {
                        new CheckerMessage($"An outdated version of Node.js was used ({used}). Consider updating. " +
                                           $"Versions supported in Oryx: {Constants.OryxGitHubUrl}")
                    };
                }
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
