// --------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// --------------------------------------------------------------------------------------------

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.Oryx.BuildScriptGenerator
{
    /// <summary>
    /// Represents a programming platform.
    /// </summary>
    public interface IProgrammingPlatform
    {
        /// <summary>
        /// Gets the name of the platform which the script generator will create builds for.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the list of versions that the script generator supports.
        /// </summary>
        IEnumerable<string> SupportedLanguageVersions { get; }

        /// <summary>
        /// Detects the programming platform name and version required by the application in source directory.
        /// </summary>
        /// <param name="sourceRepo">The <see cref="ISourceRepo"/> to detect.</param>
        /// <returns>An instance of <see cref="LanguageDetectorResult"/> if detection was
        /// successful, <c>null</c> otherwise</returns>
        LanguageDetectorResult Detect(ISourceRepo sourceRepo);

        /// <summary>
        /// Sets the version of the platform in the <see cref="ScriptGeneratorContext"/>.
        /// </summary>
        /// <param name="context">The context to set the version into.</param>
        /// <param name="version">The platform version to be set.</param>
        void SetVersion(ScriptGeneratorContext context, string version);

        /// <summary>
        /// Adds the required tools and their versions to a map.
        /// </summary>
        /// <param name="sourceRepo">The source repo for the application.</param>
        /// <param name="targetPlatformVersion">The target programming platform version that the application has requested.</param>
        /// <param name="toolsToVersion">The map from tools to their required versions.</param>
        /// <remarks>We keep the tool dependency tracking outside of the script iself to allow for
        /// scenarios where the environment already has the right tools configured and in the path,
        /// in which case no tool setup is needed.</remarks>
        void SetRequiredTools(ISourceRepo sourceRepo, string targetPlatformVersion, [NotNull] IDictionary<string, string> toolsToVersion);

        /// <summary>
        /// Tries generating a bash script based on the application in source directory.
        /// </summary>
        /// <param name="scriptGeneratorContext">The <see cref="ScriptGeneratorContext"/>.</param>
        /// <returns><see cref="BuildScriptSnippet "/> with the build snippet if successful, <c>null</c> otherwise.</returns>
        BuildScriptSnippet GenerateBashBuildScriptSnippet(ScriptGeneratorContext scriptGeneratorContext);

        /// <summary>
        /// Checks if the programming platform should be included in a build script.
        /// </summary>
        /// <param name="scriptGeneratorContext">The script generator context.</param>
        /// <returns>true if the platform should be included, false otherwise.</returns>
        bool IsEnabled(ScriptGeneratorContext scriptGeneratorContext);
    }
}