// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Php
{
    public class PhpScriptGeneratorOptions
    {
        public string PhpDefaultVersion { get; set; }

        public string InstalledPhpVersionsDir { get; set; }

        /// <summary>
        /// Gets or sets the user-provided list of python versions.
        /// </summary>
        public IList<string> SupportedPhpVersions { get; set; }
    }
}