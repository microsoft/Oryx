// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Detects language name and version of the application in source directory.
    /// </summary>
    public interface ILanguageDetector
    {
        /// <summary>
        /// Detects language name and version of the application in source directory.
        /// </summary>
        /// <param name="context">The <see cref="BuildScriptGeneratorContext"/>.</param>
        /// <returns>An instance of <see cref="LanguageDetectorResult"/> if detection was
        /// successful, <c>null</c> otherwise</returns>
        LanguageDetectorResult Detect(BuildScriptGeneratorContext context);
    }
}