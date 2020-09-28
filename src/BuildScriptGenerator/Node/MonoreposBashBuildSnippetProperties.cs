// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Node
{
    public class MonoreposBashBuildSnippetProperties
    {
        public bool HasLernaJsonFile { get; set; }

        public bool HasLageConfigJSFile { get; set; }

        public string NpmInstallLernaCommand { get; set; }

        public string LernaInitCommand { get; set; }

        public string LernaCleanCommand { get; set; }

        public string LernaListCommand { get; set; }

        public string LernaRunBuildCommand { get; set; }
    }
}
