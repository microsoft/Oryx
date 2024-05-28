// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Performs arbitrary checks during various stages of a build, in order to inform the user on failed checks.
    /// </summary>
    public interface IChecker
    {
        [NotNull]
        IEnumerable<ICheckerMessage> CheckSourceRepo(ISourceRepo repo);

        [NotNull]
        IEnumerable<ICheckerMessage> CheckToolVersions(IDictionary<string, string> toolsToVersions);
    }
}
