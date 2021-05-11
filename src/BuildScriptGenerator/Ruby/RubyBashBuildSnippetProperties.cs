// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Ruby
{
    /// <summary>
    /// Build script template for Ruby in Bash.
    /// </summary>
    internal class RubyBashBuildSnippetProperties
    {
        public bool UseBundlerToInstallDependencies { get; set; }

        public string BundlerVersion { get; set; }

        public bool GemfileExists { get; set; }

        public bool ConfigYmlFileExists { get; set; }

        public string CustomBuildCommand { get; set; }
    }
}