// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class ScriptGeneratorContext
    {
        public ISourceRepo SourceRepo { get; set; }

        public string Language { get; set; }

        public string LanguageVersion { get; set; }

        public string DestinationDir { get; set; }

        public string TempDir { get; set; }
    }
}
