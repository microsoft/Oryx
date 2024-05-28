// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public static class NodeManifestFilePropertyKeys
    {
        /// <summary>
        /// The name of the property whose value indicates the location of the output directory of a Node app.
        /// For example, 'dist' or 'public' folder.
        /// </summary>
        public const string OutputDirPath = nameof(OutputDirPath);
    }
}
