// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Text;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Performs arbitrary checks during various stages of a build, in order to inform the user on failed checks.
    /// </summary>
    public interface IChecker
    {
        IList<ICheckerMessage> CheckSourceRepo(ISourceRepo repo);

        IList<ICheckerMessage> CheckBuildScriptGeneratorOptions(BuildScriptGeneratorOptions opts);
    }
}
