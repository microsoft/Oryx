// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    [Checker(NodeConstants.NodeJsName)]
    public class NodeVersionChecker : IChecker
    {
        public IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> tools)
        {
            // check < NodeScriptGeneratorOptionsSetup.NodeLtsVersion
            //if (opts.Language == NodeConstants.NodeJsName && opts.LanguageVersion)
            return null;
        }

        public IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo)
        {
            return null;
        }
    }
}
