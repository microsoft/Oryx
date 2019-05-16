using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    [Checker(NodeConstants.NodeJsName)]
    public class NodeVersionChecker : IChecker
    {
        public IList<ICheckerMessage> CheckToolVersions(IDictionary<string, string> toolsToVersions)
        {
            // check < NodeScriptGeneratorOptionsSetup.NodeLtsVersion
            //if (opts.Language == NodeConstants.NodeJsName && opts.LanguageVersion)
            return null;
        }

        public IList<ICheckerMessage> CheckSourceRepo(ISourceRepo repo)
        {
            return null;
        }
    }
}
