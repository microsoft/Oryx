// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class ScriptGeneratorContext
    {
        public ISourceRepo SourceRepo { get; set; }

        public string LanguageName { get; set; }

        public string LanguageVersion { get; set; }

        public string OutputFolder { get; set; }

        public bool GenerateScriptOnly { get; set; }

        public string TempDirectory { get; set; }
    }
}
