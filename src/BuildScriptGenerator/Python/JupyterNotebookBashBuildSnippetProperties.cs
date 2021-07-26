// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using Microsoft.Oryx.BuildScriptGenerator.Common;
using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator.Python
{
    public class JupyterNotebookBashBuildSnippetProperties
    {
        public JupyterNotebookBashBuildSnippetProperties()
        {
            EnvironmentYmlFile = null;
            EnvironmentTemplateFileName = null;
            HasRequirementsTxtFile = false;
            NoteBookBuildCommandsFileName = FilePaths.BuildCommandsFileName;
            EnvironmentTemplatePythonVersion = null;
        }

        public JupyterNotebookBashBuildSnippetProperties(
            string environmentYmlFile,
            string environmentTemplateFileName,
            bool hasRequirementsTxtFile, 
            string environmentTemplatePythonVersion,
            string noteBookBuildCommandsFileName = null)
        {
            EnvironmentYmlFile = environmentYmlFile;
            EnvironmentTemplateFileName = environmentTemplateFileName;
            HasRequirementsTxtFile = hasRequirementsTxtFile;
            NoteBookBuildCommandsFileName = noteBookBuildCommandsFileName;
            EnvironmentTemplatePythonVersion = environmentTemplatePythonVersion;
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
