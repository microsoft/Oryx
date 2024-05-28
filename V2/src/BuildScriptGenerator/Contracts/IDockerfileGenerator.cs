// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public interface IDockerfileGenerator
    {
        /// <summary>
        /// Generates a dockerfile from the given context.
        /// </summary>
        /// <param name="ctx">The context containing information needed to generate the dockerfile.</param>
        /// <returns>The contents of the generated dockerfile.</returns>
        string GenerateDockerfile(DockerfileContext ctx);
    }
}
