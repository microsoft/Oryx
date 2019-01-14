// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    using System.Collections.Generic;

    public class NodeScriptGeneratorOptions
    {
        public string NodeJsDefaultVersion { get; set; }

        public string NpmDefaultVersion { get; set; }

        public string InstalledNodeVersionsDir { get; set; }

        public string InstalledNpmVersionsDir { get; set; }

        /// <summary>
        /// Gets or sets the list of supported NodeJs versions.
        /// </summary>
        public IList<string> SupportedNodeVersions { get; set; }

        /// <summary>
        /// Gets or sets the list of supported npm versions.
        /// </summary>
        public IList<string> SupportedNpmVersions { get; set; }
    }
}