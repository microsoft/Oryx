// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    public class ScriptGeneratorContext
    {
        public ISourceRepo SourceRepo { get; set; }

        public string Language { get; set; }

        public string LanguageVersion { get; set; }

        public string DestinationDir { get; set; }

        public IDictionary<string, string> Properties { get; set; }
    }
}