// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// Build script template for DotnetCore in Bash.
    /// </summary>
    public class DotNetCoreBashBuildSnippetProperties
    {
        public string ProjectFile { get; set; }

        public string PublishDirectory { get; set; }

        public IEnumerable<string> DirectoriesToExcludeFromCopyToIntermediateDir { get; set; }

        public string BenvArgs { get; set; }

        public string PreBuildCommand { get; set; }

        public string PostBuildCommand { get; set; }

        public bool ZipAllOutput { get; set; }

        public string ManifestFileName { get; set; }

        public Dictionary<string, string> BuildProperties { get; set; }

        public string Configuration { get; set; }
    }
}