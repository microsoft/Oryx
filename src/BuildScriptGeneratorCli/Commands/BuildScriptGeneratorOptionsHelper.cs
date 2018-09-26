// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

using System;
using System.IO;
using Microsoft.Oryx.BuildScriptGenerator;

namespace Microsoft.Oryx.BuildScriptGeneratorCli
{
    internal static class BuildScriptGeneratorOptionsHelper
    {
        public static void ConfigureBuildScriptGeneratorOptions(
            BuildScriptGeneratorOptions options,
            string sourceCodeFolder,
            string languageName,
            string languageVersion)
        {
            ConfigureBuildScriptGeneratorOptions(
                options,
                sourceCodeFolder,
                outputFolder: null,
                intermediateFolder: null,
                doNotUseIntermediateFolder: false,
                languageName,
                languageVersion);
        }

        public static void ConfigureBuildScriptGeneratorOptions(
            BuildScriptGeneratorOptions options,
            string sourceCodeFolder,
            string outputFolder,
            string intermediateFolder,
            bool doNotUseIntermediateFolder,
            string languageName,
            string languageVersion)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.SourceCodeFolder = Path.GetFullPath(sourceCodeFolder);
            options.LanguageName = languageName;
            options.LanguageVersion = languageVersion;

            // Create one unique subdirectory per session (or run of this tool)
            // Example structure:
            // /tmp/BuildScriptGenerator/guid1
            // /tmp/BuildScriptGenerator/guid2
            options.TempDirectory = Path.Combine(
                Path.GetTempPath(),
                nameof(BuildScriptGenerator),
                Guid.NewGuid().ToString("N"));

            if (!string.IsNullOrEmpty(outputFolder))
            {
                options.OutputFolder = Path.GetFullPath(outputFolder);
            }

            if (!string.IsNullOrEmpty(intermediateFolder))
            {
                options.IntermediateFolder = Path.GetFullPath(intermediateFolder);
            }

            options.DoNotUseIntermediateFolder = doNotUseIntermediateFolder;
        }
    }
}
