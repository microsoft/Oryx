// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Represents the result of a <see cref="ILanguageDetector.Detect(ISourceRepo)"/> operation.
    /// </summary>
    public class LanguageDetectorResult
    {
        public string Language { get; set; }

        public string LanguageVersion { get; set; }
    }
}
