// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class JupyterNotebookBashBuildSnippetProperties
    {
        public JupyterNotebookBashBuildSnippetProperties()
        {
            this.EnvironmentYmlFile = null;
            this.EnvironmentTemplateFileName = null;
            this.HasRequirementsTxtFile = false;
            this.NoteBookBuildCommandsFileName = FilePaths.BuildCommandsFileName;
            this.EnvironmentTemplatePythonVersion = null;
        }

        public JupyterNotebookBashBuildSnippetProperties(
            string environmentYmlFile,
            string environmentTemplateFileName,
            bool hasRequirementsTxtFile,
            string environmentTemplatePythonVersion,
            string noteBookBuildCommandsFileName = null)
        {
            this.EnvironmentYmlFile = environmentYmlFile;
            this.EnvironmentTemplateFileName = environmentTemplateFileName;
            this.HasRequirementsTxtFile = hasRequirementsTxtFile;
            this.NoteBookBuildCommandsFileName = noteBookBuildCommandsFileName;
            this.EnvironmentTemplatePythonVersion = environmentTemplatePythonVersion;
        }

        public string EnvironmentYmlFile { get; set; }

        public string EnvironmentTemplateFileName { get; set; }

        public string EnvironmentTemplatePythonVersion { get; set; }

        public bool HasRequirementsTxtFile { get; set; }

        /// <summary>
        /// Gets or sets the name of the build commands file for conda.
        /// </summary>
        public string NoteBookBuildCommandsFileName { get; set; }
    }
}
