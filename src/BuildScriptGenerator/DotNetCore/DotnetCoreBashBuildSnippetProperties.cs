// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.Oryx.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.DotNetCore
{
    /// <summary>
    /// Build script template for DotNetCore in Bash.
    /// </summary>
    public class DotNetCoreBashBuildSnippetProperties
    {
        public const string PreBuildCommandPrologue = Constants.PreBuildCommandPrologue;
        public const string PreBuildCommandEpilogue = Constants.PreBuildCommandEpilogue;

        public const string PostBuildCommandPrologue = Constants.PostBuildCommandPrologue;
        public const string PostBuildCommandEpilogue = Constants.PostBuildCommandEpilogue;

        public string ProjectFile { get; set; }

        public string PublishDirectory { get; set; }

        public IEnumerable<string> DirectoriesToExcludeFromCopyToIntermediateDir { get; set; }

        public string BenvArgs { get; set; }

        public string PreBuildCommand { get; set; }

        public string PostBuildCommand { get; set; }

        public bool ZipAllOutput { get; set; }

        public string ZippedOutputFileName { get; set; } = FilePaths.CompressedOutputFileName;

        public string ManifestFileName { get; set; }

        public string ManifestDir { get; set; }

        public Dictionary<string, string> BuildProperties { get; set; }

        public string Configuration { get; set; }
    }
}