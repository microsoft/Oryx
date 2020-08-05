// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class JupyterNotebookBashBuildSnippetProperties
    {
        public string EnvironmentYmlFile { get; set; }

        public string EnvironmentTemplateFileName { get; set; }

        public string EnvironmentTemplatePythonVersion { get; set; }

        public bool HasRequirementsTxtFile { get; set; }
    }
}
